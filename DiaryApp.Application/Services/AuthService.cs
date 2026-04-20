using DiaryApp.Application.DTOs.Auth;
using DiaryApp.Application.Interfaces;
using DiaryApp.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using DiaryApp.Application.DTOs;
using Google.Apis.Auth;
using DiaryApp.Application.Interfaces.Services;

namespace DiaryApp.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IEmailService emailService,
    IJwtProvider jwtProvider,
    IGoogleAuthProvider googleAuthProvider,
    IRedisCacheService cacheService
) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IEmailService _emailService = emailService;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly IGoogleAuthProvider _googleAuthProvider = googleAuthProvider;
    private readonly IRedisCacheService _cacheService = cacheService;

    private string GetOtpKey(string email)
    {
        return $"auth:otp:{email.ToLower().Trim()}";
    } 

    private string GetResetTokenKey(string email)
    {
        return $"auth:reset_token:{email.ToLower().Trim()}";
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var email = request.Email.Trim().ToLower();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException("User name cannot be empty.");
        } 

        bool userExists = await _userRepository.ExistsByEmailAsync(request.Email);
        if (userExists)
        {
            throw new InvalidOperationException("This email is already in use. Please try another one!");
        }

        string hashPassword = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 8);
        User newUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            Name = name,
            HashPassword = hashPassword,
            AuthProvider = "Local",
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(newUser);

        return new AuthResponseDto
        {
            Token = _jwtProvider.GenerateToken(newUser),
            UserId = newUser.Id,
            Name = newUser.Name,
            AvatarUrl = newUser.AvatarUrl
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim().ToLower();

        string cacheKey = $"auth:email:{email}";
        var user = await _cacheService.GetAsync<User>(cacheKey);

        if (user == null)
        {
            user = await _userRepository.GetByEmailAsync(email);
            
            if (user != null)
            {
                await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(15));
            }
        }

        if (user == null) throw new UnauthorizedAccessException("Incorrect email or password.");

        if (user.AuthProvider != "Local") 
        {
            throw new Exception("This account was registered via Google. Please log in with Google instead.");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.HashPassword);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Incorrect email or password.");
        }

        return new AuthResponseDto
        {
            Token = _jwtProvider.GenerateToken(user),
            UserId = user.Id,
            Name = user.Name ?? "username",
            AvatarUrl = user.AvatarUrl
        };
    }

    public async Task<AuthResponseDto> LoginWithGoogleAsync(GoogleLoginRequestDto request)
    {
        var payload = await _googleAuthProvider.ValidateTokenAsync(request.IdToken);
        return await HandleSocialLogin(payload.Email, payload.Name, payload.Picture);
    }

    private async Task<AuthResponseDto> HandleSocialLogin(string email, string name, string picture)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                Name = name,
                HashPassword = "",
                AvatarUrl = picture,
                AuthProvider = "Google",
                CreatedAt = DateTime.UtcNow
            };
            await _userRepository.CreateAsync(user);
        }

        return new AuthResponseDto
        {
            Token = _jwtProvider.GenerateToken(user),
            UserId = user.Id,
            Name = user.Name ?? "username",
            AvatarUrl = user.AvatarUrl
        };
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var email = request.Email.Trim().ToLower();
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null || user.AuthProvider != "Local") throw new KeyNotFoundException("We couldn't find an account with that email.");

        string otp = Random.Shared.Next(100000, 999999).ToString();
        user.ResetOtp = otp;
        string otpCacheKey = $"auth:otp:{email}";
        await _cacheService.SetAsync(otpCacheKey, otp, TimeSpan.FromMinutes(10));

        string subject = $"[{otp}] Password Recovery Code";
        string emailBody = $@"
            <h2>Password Reset Request</h2>
            <p>Your OTP code is: <b style='font-size: 24px; color: #4CAF50;'>{otp}</b></p>
            <p>This code will expire in 10 minutes. If you didn't request this, you can safely ignore this email.</p>";

        _ = Task.Run(async () => 
        {
            try 
            {
                await _emailService.SendEmailAsync(user.Email, subject, emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email delivery failed: {ex.Message}");
            }
        }); 
    }

    public async Task<VerifyOtpResponseDto> VerifyOtpAndGenerateTokenAsync(VerifyOtpRequestDto request)
    {
        var otpCacheKey = GetOtpKey(request.Email);
        var savedOtp = await _cacheService.GetAsync<string>(otpCacheKey);

        if (string.IsNullOrEmpty(savedOtp) || savedOtp != request.OtpCode)
        {
            throw new UnauthorizedAccessException("The OTP code is incorrect or has expired.");
        }

        await _cacheService.RemoveAsync(otpCacheKey);

        var resetToken = new VerifyOtpResponseDto
        {
            ResetToken = Guid.NewGuid().ToString("N")
        };
        var tokenCacheKey = GetResetTokenKey(request.Email);
        
        await _cacheService.SetAsync(tokenCacheKey, resetToken, TimeSpan.FromMinutes(10));

        return resetToken;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var tokenCacheKey = GetResetTokenKey(request.Email);
        var savedToken = await _cacheService.GetAsync<string>(tokenCacheKey);

        if (string.IsNullOrEmpty(savedToken) || savedToken != request.ResetToken)
        {
            throw new UnauthorizedAccessException("Invalid or expired session. Please try again.");
        }

        var email = request.Email.Trim().ToLower();
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null || user.AuthProvider != "Local")
        {
            throw new KeyNotFoundException("Email not found in our system.");
        }

        user.HashPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 8);
        await _userRepository.UpdateAsync(user);

        await _cacheService.RemoveAsync(tokenCacheKey);
        await _cacheService.RemoveAsync($"auth:email:{email}");
    }
}
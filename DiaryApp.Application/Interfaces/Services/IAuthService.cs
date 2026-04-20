
using DiaryApp.Application.DTOs;
using DiaryApp.Application.DTOs.Auth;

namespace DiaryApp.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> LoginWithGoogleAsync(GoogleLoginRequestDto request);
    Task ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<VerifyOtpResponseDto> VerifyOtpAndGenerateTokenAsync(VerifyOtpRequestDto request);
    Task ResetPasswordAsync(ResetPasswordRequestDto request);
}
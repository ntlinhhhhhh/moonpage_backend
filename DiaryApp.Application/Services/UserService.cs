using DiaryApp.Application.DTOs.Queue;
using DiaryApp.Application.DTOs.User;
using DiaryApp.Application.Interfaces;
using DiaryApp.Application.Interfaces.Services;
using DiaryApp.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace DiaryApp.Application.Services;

public class UserService(
    IUserRepository userRepository,
    IThemeRepository themeRepository,
    IUserStreakRepository userStreakRepository,
    IRedisCacheService cacheService,
    IMessageProducer messageProducer,
    IGoogleStorageService googleStorageService,
    IGoogleAuthProvider googleAuthProvider
    ) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IThemeRepository _themeRepository = themeRepository;
    private readonly IUserStreakRepository _userStreakRepository = userStreakRepository;
    private readonly IRedisCacheService _cacheService = cacheService;
    private readonly IMessageProducer _messageProducer = messageProducer;
    private readonly IGoogleStorageService _googleStorageService = googleStorageService;
    private readonly IGoogleAuthProvider _googleAuthProvider = googleAuthProvider;
    
    public async Task<UserProfileDto> GetProfileAsync(string userId)
    {
        string cacheKey = $"user_profile:{userId}";
        var cachedUser = await _cacheService.GetAsync<UserProfileDto>(cacheKey);

        if (cachedUser != null) return cachedUser;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("We couldn't find your profile information.");
        }

        var streak = await _userStreakRepository.GetByUserIdAsync(userId);
        
        var profile = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            AvatarUrl = _googleStorageService.GetImageUrl(user.AvatarUrl),
            Gender = user.Gender,
            Birthday = user.Birthday,
            CoinBalance = user.CoinBalance,
            CurrentStreak = streak?.CurrentStreak ?? 0,
            LongestStreak = streak?.LongestStreak ?? 0,
            StreakFreezes = streak?.StreakFreezes ?? 0,
            RecoverableStreak = streak?.RecoverableStreak ?? 0,
            AuthProvider = user.AuthProvider,
            ActiveThemeId = user.ActiveThemeId,
            CreatedAt = user.CreatedAt
        };

        await _cacheService.SetAsync(cacheKey, profile, TimeSpan.FromMinutes(30));
        return profile;
    }

    public async Task UpdateProfileAsync(string userId, UpdateProfileRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null) throw new KeyNotFoundException("This user account does not exist.");

        await _userRepository.UpdateProfileAsync(
            userId: userId,
            name: request.Name.Trim(),
            newPassword: null,
            avatarUrl: request.AvatarUrl,
            gender: request.Gender,
            birthday: request.Birthday
        );

        var keysToRemove = new List<string>
        {
            $"user_profile:{userId}",
            $"auth:email:{user.Email}"
        };
        await Task.WhenAll(keysToRemove.Select(key => _cacheService.RemoveAsync(key)));

        var payload = new DatabaseTaskPayload
        {
            TaskType = DbTaskType.SyncUserMedia,
            UserId = userId,
            UserName = request.Name.Trim(),
            AvatarUrl = request.AvatarUrl
        };
        await _messageProducer.SendMessageAsync(payload, "db_tasks_queue");
    }

    public async Task UploadAvatarAsync(string userId, UploadAvatarRequestDto request)
    {
        var tempPath = await SaveTempFileAsync(request.ImageFile, userId);

        var payload = new ImageUploadPayload
        {
            UserId = userId,
            EntityId = userId,
            UploadType = ImageUploadType.Avatar,
            TempImagePath = tempPath
        };

        await _messageProducer.SendMessageAsync(payload, "image_upload_queue");
    }

    private async Task<string> SaveTempFileAsync(IFormFile file, string userId)
    {
        var tempFolder = Path.Combine(Path.GetTempPath(), "moonpage_temp_images", "avatars");
        if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);

        var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(tempFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return filePath;
    }

    public async Task UpdateAvatarUrlAsync(string userId, string imageUrl)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        await _userRepository.UpdateProfileAsync(
            userId: userId,
            name: user.Name,
            newPassword: null,
            avatarUrl: imageUrl,
            gender: user.Gender,
            birthday: user.Birthday
        );

        var keysToRemove = new List<string> { $"user_profile:{userId}", $"auth:email:{user.Email}" };
        await Task.WhenAll(keysToRemove.Select(k => _cacheService.RemoveAsync(k)));

        var syncPayload = new DatabaseTaskPayload
        {
            TaskType = DbTaskType.SyncUserMedia,
            UserId = userId,
            UserName = user.Name,
            AvatarUrl = imageUrl
        };
        await _messageProducer.SendMessageAsync(syncPayload, "db_tasks_queue");
    }

    public async Task<IEnumerable<UserSearchResponseDto>> SearchUsersAsync(string name, int limit)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException("Please enter a keyword to search.");

        var users = await _userRepository.SearchByNameAsync(name, limit);

        return users.Select(u => new UserSearchResponseDto
        {
            Id = u.Id,
            Name = u.Name,
            AvatarUrl = u.AvatarUrl,
            Email = u.Email
        });
    }

    public async Task<(bool IsSuccess, string Message)> BuyThemeAsync(string userId, BuyThemeRequestDto request)
    {
        var theme = await _themeRepository.GetByIdAsync(request.ThemeId);
        if (theme == null || !theme.IsActive) 
        {
            return (false, "This theme isn't available or has been discontinued.");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) 
        {
            return (false, "We couldn't find your account info.");
        }

        var ownedThemes = await _userRepository.GetOwnedThemeIdsAsync(userId);
        if (ownedThemes.Contains(request.ThemeId))
        {
            return (false, "You already own this theme!");
        }

        if (user.CoinBalance < request.Price)
        {
            return (false, $"You don't have enough coins. You need {request.Price} coins to purchase this theme.");
        }

        await _userRepository.UpdateCoinBalanceAsync(userId, -request.Price);
        await _userRepository.AddOwnedThemeAsync(userId, request.ThemeId);

        var keysToRemove = new List<string>
        {
            $"user_profile:{userId}",
            $"owned_themes:{userId}"
        };
        await Task.WhenAll(keysToRemove.Select(key => _cacheService.RemoveAsync(key)));

        return (true, "Theme purchased successfully!");
    }

    public async Task<(bool IsSuccess, string Message)> BuyStreakFreezeAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        int freezePrice = 200;

        if (user == null || user.CoinBalance < freezePrice)
        {
            return (false, $"You don't have enough coins. You need {freezePrice} coins to purchase a Streak Freeze.");
        }

        var streak = await _userStreakRepository.GetByUserIdAsync(userId) 
                    ?? new UserStreak { UserId = userId };

        streak.StreakFreezes += 1;

        await _userRepository.UpdateCoinBalanceAsync(userId, -freezePrice);
        await _userStreakRepository.UpsertAsync(streak);

        await _cacheService.RemoveAsync($"user_profile:{userId}");

        return (true, "Streak Freeze purchased successfully! Your streak is now protected.");
    }

    public async Task<(bool IsSuccess, string Message)> RecoverStreakAsync(string userId)
    {
        var streak = await _userStreakRepository.GetByUserIdAsync(userId);

        if (streak == null || streak.RecoverableStreak <= 0)
        {
            return (false, "There is no streak to recover or your streak is already active.");
        }

        if (streak.StreakFreezes <= 0)
        {
            return (false, "You don't have any Streak Freezes. Please buy one in the store.");
        }

        // Recover the streak: Old streak + the log they just made (which is why CurrentStreak was 1)
        // If CurrentStreak was already > 1, it means they might have logged again, but let's assume 
        // they recover right after it resets to 1.
        streak.CurrentStreak = streak.RecoverableStreak + (streak.CurrentStreak > 0 ? 0 : 1); 
        
        // Actually, logic in Worker sets CurrentStreak to 1 when it resets.
        // So Recovered Streak should be RecoverableStreak (which was the count before reset)
        // Wait, if they log today and it resets to 1. The 1 IS the today log.
        // So Total Streak = Old Streak + 1.
        streak.CurrentStreak = streak.RecoverableStreak; 
        // Correction: The Worker already incremented/reset. 
        // If lastLog was 2 days ago, Worker sets CurrentStreak = 1.
        // That 1 represents the log of "Today".
        // So if they had 10 days, missed yesterday, log today -> Worker sets CurrentStreak = 1, Recoverable = 10.
        // Recovering should make it 11? No, a Freeze "fills the gap".
        // So it's as if they didn't miss. 10 + 1 = 11.
        streak.CurrentStreak = streak.RecoverableStreak + 1;
        
        if (streak.CurrentStreak > streak.LongestStreak)
            streak.LongestStreak = streak.CurrentStreak;

        streak.StreakFreezes -= 1;
        streak.RecoverableStreak = 0;
        streak.UpdatedAt = DateTime.UtcNow;

        await _userStreakRepository.UpsertAsync(streak);
        await _cacheService.RemoveAsync($"user_profile:{userId}");

        return (true, $"Streak recovered! Your current streak is now {streak.CurrentStreak} days.");
    }

    public async Task ChangeActiveThemeAsync(string userId, UpdateThemeRequestDto request)
    {
        var theme = await _themeRepository.GetByIdAsync(request.ThemeId);
        if (theme == null || !theme.IsActive)
        {
            throw new KeyNotFoundException("This theme is invalid or has been removed from the store.");
        }
        
        var ownedThemes = await _userRepository.GetOwnedThemeIdsAsync(userId);
        if (!ownedThemes.Contains(request.ThemeId)) 
        {
            throw new Exception("You need to purchase this theme before you can use it.");
        }

        await _userRepository.SetActiveThemeAsync(userId, request.ThemeId);
        await _cacheService.RemoveAsync($"user_profile:{userId}");
    }

    public async Task<List<string>> GetMyThemeIdsAsync(string userId)
    {
        string cacheKey = $"owned_themes:{userId}";

        var cachedThemeIds = await _cacheService.GetAsync<List<string>>(cacheKey);
        if (cachedThemeIds != null) return cachedThemeIds;

        var themeIds = await _userRepository.GetOwnedThemeIdsAsync(userId);
        await _cacheService.SetAsync(cacheKey, themeIds, TimeSpan.FromHours(1));

        return themeIds;
    }

    public async Task DeleteUserAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("The user you are trying to delete could not be found.");
        }
        
        await _userRepository.DeleteAsync(userId);

        var keysToRemove = new List<string>
        {
            $"user_profile:{userId}",
            $"auth:email:{user.Email}",
            $"owned_themes:{userId}"
        };
        await Task.WhenAll(keysToRemove.Select(key => _cacheService.RemoveAsync(key)));
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        if (user.AuthProvider != "Local")
        {
            throw new InvalidOperationException("This account is linked with Google. Password cannot be changed manually.");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.HashPassword);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("The old password you entered is incorrect.");
        }

        await _userRepository.UpdateProfileAsync(
            userId: userId,
            name: user.Name,
            newPassword: request.NewPassword,
            avatarUrl: user.AvatarUrl,
            gender: user.Gender,
            birthday: user.Birthday
        );

        await _cacheService.RemoveAsync($"auth:email:{user.Email}");
    }

    public async Task<bool> ConfirmPasswordAsync(string userId, ConfirmPasswordRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        if (user.AuthProvider != "Local")
        {
            if (string.IsNullOrEmpty(request.GoogleIdToken))
            {
                throw new ArgumentException("Google ID token is required to confirm this account.");
            }

            var payload = await _googleAuthProvider.ValidateTokenAsync(request.GoogleIdToken);
            return payload.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase);
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            throw new ArgumentException("Password is required to confirm this account.");
        }

        return BCrypt.Net.BCrypt.Verify(request.Password, user.HashPassword);
    }

    public async Task<IEnumerable<UserSearchResponseDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return users.Select(u => new UserSearchResponseDto
        {
            Id = u.Id,
            Name = u.Name,
            AvatarUrl = u.AvatarUrl,
            Email = u.Email
        });
    }
}
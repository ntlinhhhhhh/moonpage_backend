using DiaryApp.Application.DTOs.User;
using DiaryApp.Application.Interfaces;
using DiaryApp.Application.Interfaces.Services;
using DiaryApp.Domain.Entities;

namespace DiaryApp.Application.Services;

public class UserService(
    IUserRepository userRepository,
    IThemeRepository themeRepository,
    IMomentRepository momentRepository,
    IUserStreakRepository userStreakRepository,
    IRedisCacheService cacheService
    ) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IThemeRepository _themeRepository = themeRepository;
    private readonly IMomentRepository _momentRepository = momentRepository;
    private readonly IUserStreakRepository _userStreakRepository = userStreakRepository;
    private readonly IRedisCacheService _cacheService = cacheService;

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
        
        var profile = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            AvatarUrl = user.AvatarUrl,
            Gender = user.Gender,
            Birthday = user.Birthday,
            CoinBalance = user.CoinBalance,
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

        await _cacheService.RemoveAsync($"user_profile:{userId}");
        await _cacheService.RemoveAsync($"auth:email:{user.Email}");

        _ = Task.Run(async () => 
        {
            try
            {
                await _momentRepository.SyncUserMediaInMomentsAsync(userId, request.Name, request.AvatarUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error syncing moments for user {userId}: {ex.Message}");
            }
        });
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

        await Task.WhenAll(
            _userRepository.UpdateCoinBalanceAsync(userId, -request.Price),
            _userRepository.AddOwnedThemeAsync(userId, request.ThemeId),
            _cacheService.RemoveAsync($"user_profile:{userId}"),
            _cacheService.RemoveAsync($"owned_themes:{userId}")
        );

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

        await Task.WhenAll(
            _userRepository.UpdateCoinBalanceAsync(userId, -freezePrice),
            _userStreakRepository.UpsertAsync(streak)
        );

        return (true, "Streak Freeze purchased successfully! Your streak is now protected.");
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
        await _cacheService.RemoveAsync($"user_profile:{userId}");
        await _cacheService.RemoveAsync($"auth:email:{user.Email}");
        await _cacheService.RemoveAsync($"owned_themes:{userId}");
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
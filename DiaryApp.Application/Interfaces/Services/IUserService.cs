using DiaryApp.Application.DTOs.User;

namespace DiaryApp.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(string userId);
    Task UpdateProfileAsync(string userId, UpdateProfileRequestDto request);
    Task UploadAvatarAsync(string userId, UploadAvatarRequestDto request);
    Task UpdateAvatarUrlAsync(string userId, string avatarUrl);
    Task<IEnumerable<UserSearchResponseDto>> SearchUsersAsync(string name, int limit);
    Task<List<string>> GetMyThemeIdsAsync(string userId);
    Task ChangeActiveThemeAsync(string userId, UpdateThemeRequestDto request);
    Task<(bool IsSuccess, string Message)> BuyThemeAsync(string userId, BuyThemeRequestDto request);
    Task<(bool IsSuccess, string Message)> BuyStreakFreezeAsync(string userId);
    Task<IEnumerable<UserSearchResponseDto>> GetAllUsersAsync();
    Task DeleteUserAsync(string userId);
    Task ChangePasswordAsync(string userId, ChangePasswordRequestDto request);
    Task<bool> ConfirmPasswordAsync(string userId, ConfirmPasswordRequestDto request);
}
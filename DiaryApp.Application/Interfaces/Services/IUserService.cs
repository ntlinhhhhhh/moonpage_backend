using DiaryApp.Application.DTOs.User;

namespace DiaryApp.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(string userId);
    Task UpdateProfileAsync(string userId, UpdateProfileRequestDto request);
    Task<IEnumerable<UserSearchResponseDto>> SearchUsersAsync(string name, int limit);
    Task<List<string>> GetMyThemeIdsAsync(string userId);
    Task ChangeActiveThemeAsync(string userId, UpdateThemeRequestDto request);
    Task BuyThemeAsync(string userId, BuyThemeRequestDto request);

    Task<IEnumerable<UserSearchResponseDto>> GetAllUsersAsync();
    Task DeleteUserAsync(string userId);

}
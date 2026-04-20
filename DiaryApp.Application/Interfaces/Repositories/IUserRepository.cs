using DiaryApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiaryApp.Application.Interfaces;

public interface IUserRepository
{
    // search --> extend friendship
    Task <IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> SearchByNameAsync(string name, int limit = 10);
    Task<bool> ExistsByEmailAsync(string email); // check exist

    // profile management
    Task CreateAsync(User user);
    Task UpdateProfileAsync(string userId, string name, string newPassword, string? avatarUrl, string? gender, string? birthday);
    Task UpdateAsync(User user);
    Task DeleteAsync(string userId);

    // store and coin
    Task UpdateCoinBalanceAsync(string userId, int amount);
    Task AddOwnedThemeAsync(string userId, string themeId);
    Task SetActiveThemeAsync(string userId, string themeId);
    Task<List<string>> GetOwnedThemeIdsAsync(string userId); // list themes of user
}
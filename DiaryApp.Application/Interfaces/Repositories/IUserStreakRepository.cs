using DiaryApp.Domain.Entities;

namespace DiaryApp.Application.Interfaces;

public interface IUserStreakRepository
{
    Task<UserStreak?> GetByUserIdAsync(string userId);
    Task UpsertAsync(UserStreak streak);
}
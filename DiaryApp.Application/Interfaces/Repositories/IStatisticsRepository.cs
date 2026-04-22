using DiaryApp.Domain.Entities;

namespace DiaryApp.Application.Interfaces.Repositories;

public interface IStatisticsRepository
{
    Task<int> GetTotalPhotosCountAsync(string userId);
    
    Task<List<DailyLog>> GetLogsInRangeAsync(string userId, int year, int? month);
}
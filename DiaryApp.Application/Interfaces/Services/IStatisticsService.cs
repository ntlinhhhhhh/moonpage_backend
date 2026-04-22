using DiaryApp.Application.DTOs.Statistic;

namespace DiaryApp.Application.Interfaces.Services;

public interface IStatisticsService
{
    Task<UserStatsSummaryDto?> GetStatsSummaryAsync(string userId, int year, int? month);
}
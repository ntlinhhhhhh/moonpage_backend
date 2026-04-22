using DiaryApp.Application.DTOs.Statistic;
using DiaryApp.Application.Interfaces;
using DiaryApp.Application.Interfaces.Repositories;
using DiaryApp.Application.Interfaces.Services;
using DiaryApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DiaryApp.Application.Services;

public class StatisticsService(
    IStatisticsRepository statsRepo, 
    IUserStreakRepository streakRepo, 
    IActivityRepository activityRepo,
    ILogger<StatisticsService> logger
) : IStatisticsService
{
    public async Task<UserStatsSummaryDto?> GetStatsSummaryAsync(string userId, int year, int? month)
    {
        try
        {
            var logsTask = statsRepo.GetLogsInRangeAsync(userId, year, month);
            var streakTask = streakRepo.GetByUserIdAsync(userId);
            var activitiesTask = activityRepo.GetAllAsync();
            var photosTask = statsRepo.GetTotalPhotosCountAsync(userId);

            await Task.WhenAll(logsTask, streakTask, activitiesTask, photosTask);

            var logs = logsTask.Result;
            var streak = streakTask.Result;
            var allActivities = activitiesTask.Result;
            var totalPhotos = photosTask.Result;

            var logsWithMood = logs.Where(l => l.BaseMoodId.HasValue).ToList();
            var moodDist = logsWithMood.GroupBy(l => l.BaseMoodId!.Value)
                .Select(g => new MoodDistributionDto {
                    Label = ((BaseMood)g.Key).ToString(),
                    Count = g.Count(),
                    Percentage = logsWithMood.Count > 0 
                        ? Math.Round((double)g.Count() / logsWithMood.Count * 100, 1) 
                        : 0
                }).ToList();

            var influences = new List<ActivityInfluenceDto>();
            foreach (var act in allActivities)
            {
                var relatedLogs = logs.Where(l => l.ActivityIds != null && l.ActivityIds.Contains(act.Id) && l.BaseMoodId.HasValue).ToList();
                if (relatedLogs.Any())
                {
                    influences.Add(new ActivityInfluenceDto {
                        ActivityId = act.Id,
                        ActivityName = act.Name,
                        IconUrl = act.IconUrl,
                        AverageMoodScore = Math.Round(relatedLogs.Average(l => (int)l.BaseMoodId!), 2),
                        Occurrence = relatedLogs.Count
                    });
                }
            }

            logger.LogInformation("Successfully processed stats for User {UserId} ({Year}-{Month}).", userId, year, month);

            return new UserStatsSummaryDto {
                TotalLogs = logs.Count,
                TotalPhotos = totalPhotos,
                CurrentStreak = streak?.CurrentStreak ?? 0,
                LongestStreak = streak?.LongestStreak ?? 0,
                MoodDistribution = moodDist.OrderByDescending(m => m.Count).ToList(),
                MoodFlow = logsWithMood.OrderBy(l => l.Date).Select(l => new MoodFlowDto { 
                    Date = l.Date, 
                    MoodId = l.BaseMoodId!.Value 
                }).ToList(),
                BestActivities = influences.OrderByDescending(x => x.AverageMoodScore).Take(5).ToList()
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "System error while calculating statistics for user {UserId}.", userId);
            return null;
        }
    }
}
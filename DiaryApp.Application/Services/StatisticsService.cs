using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    ILogger<StatisticsService> logger,
    IRedisCacheService cacheService
) : IStatisticsService
{
    private readonly IStatisticsRepository _statsRepo = statsRepo;
    private readonly IUserStreakRepository _streakRepo = streakRepo;
    private readonly IActivityRepository _activityRepo = activityRepo;
    private readonly ILogger<StatisticsService> _logger = logger;
    private readonly IRedisCacheService _cacheService = cacheService;
    
    private readonly string _cachePrefix = "stats_summary";
    private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(15);

    public async Task<UserStatsSummaryDto?> GetStatsSummaryAsync(string userId, int year, int? month)
    {
        try
        {
            // 1. Tạo Cache Key duy nhất
            string cacheKey = $"{_cachePrefix}:{userId}:{year}:{month ?? 0}";

            // 2. Kiểm tra Redis Cache
            var cachedStats = await _cacheService.GetAsync<UserStatsSummaryDto>(cacheKey);
            if (cachedStats != null)
            {
                _logger.LogInformation("Retrieved stats from Redis cache for User {UserId} ({Year}-{Month}).", userId, year, month);
                return cachedStats;
            }

            // 3. Gọi song song để tối ưu tốc độ
            var logsTask = _statsRepo.GetLogsInRangeAsync(userId, year, month);
            var streakTask = _streakRepo.GetByUserIdAsync(userId);
            var activitiesTask = _activityRepo.GetAllAsync();
            var photosTask = _statsRepo.GetTotalPhotosCountAsync(userId);

            await Task.WhenAll(logsTask, streakTask, activitiesTask, photosTask);

            var logs = logsTask.Result;
            var streak = streakTask.Result;
            var allActivities = activitiesTask.Result;
            var totalPhotos = photosTask.Result;

            // 4. Xử lý dữ liệu
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

            // 5. Đóng gói kết quả
            var result = new UserStatsSummaryDto {
                TotalLogs = logs.Count,
                TotalPhotos = totalPhotos,
                CurrentStreak = streak?.CurrentStreak ?? 0,
                LongestStreak = streak?.LongestStreak ?? 0,
                MoodDistribution = moodDist.OrderByDescending(m => m.Count).ToList(),
                MoodFlow = logsWithMood.OrderBy(l => l.Date).Select(l => new MoodFlowDto { 
                    Date = l.Date, 
                    MoodId = l.BaseMoodId!.Value 
                }).ToList(),
                BestActivities = influences.OrderByDescending(x => x.AverageMoodScore).Take(5).ToList(),

                // New stats calculation
                TotalSteps = logs.Sum(l => l.Steps),
                AverageSteps = logs.Any() ? (int)logs.Average(l => l.Steps) : 0,
                TotalCalories = logs.Sum(l => l.Calories),
                AverageCalories = logs.Any() ? (int)logs.Average(l => l.Calories) : 0,
                TotalDistance = Math.Round(logs.Sum(l => l.Distance), 2),
                AverageDistance = logs.Any() ? Math.Round(logs.Average(l => l.Distance), 2) : 0,
                AverageSleepHours = logs.Any(l => l.SleepHours > 0) 
                    ? Math.Round(logs.Where(l => l.SleepHours > 0).Average(l => l.SleepHours), 1) 
                    : 0,
                SleepAnalysis = logs.Where(l => l.SleepHours > 0 || !string.IsNullOrEmpty(l.SleepStartTime) || !string.IsNullOrEmpty(l.WakeupTime))
                    .Select(l => new SleepDataDto {
                        Date = l.Date,
                        SleepStartTime = l.SleepStartTime,
                        WakeupTime = l.WakeupTime,
                        Duration = l.SleepHours,
                        MoodId = l.BaseMoodId
                    }).ToList(),
                MusicSummary = logs.Where(l => !string.IsNullOrEmpty(l.MusicRecord))
                    .Select(l => l.MusicRecord!)
                    .ToList()
            };

            // Calculate Average Sleep Start Time
            var sleepStartTimes = logs.Where(l => !string.IsNullOrEmpty(l.SleepStartTime)).ToList();
            if (sleepStartTimes.Any())
            {
                // Note: This is a simple average, might not handle "around midnight" perfectly (e.g., 23:00 and 01:00)
                // but for basic stats it's often acceptable. For better accuracy, specialized circular mean would be needed.
                double totalMinutes = 0;
                foreach (var log in sleepStartTimes)
                {
                    if (TimeSpan.TryParse(log.SleepStartTime, out var ts))
                    {
                        totalMinutes += ts.TotalMinutes;
                    }
                }
                var avgMinutes = totalMinutes / sleepStartTimes.Count;
                var avgTime = TimeSpan.FromMinutes(avgMinutes);
                result.AverageSleepStartTime = avgTime.ToString(@"hh\:mm"); 
            }

            // Calculate Average Wakeup Time
            var wakeupTimes = logs.Where(l => !string.IsNullOrEmpty(l.WakeupTime)).ToList();
            if (wakeupTimes.Any())
            {
                double totalMinutes = 0;
                foreach (var log in wakeupTimes)
                {
                    if (TimeSpan.TryParse(log.WakeupTime, out var ts))
                    {
                        totalMinutes += ts.TotalMinutes;
                    }
                }
                var avgMinutes = totalMinutes / wakeupTimes.Count;
                var avgTime = TimeSpan.FromMinutes(avgMinutes);
                result.AverageWakeupTime = avgTime.ToString(@"hh\:mm");
            }

            // 6. Lưu vào Cache
            await _cacheService.SetAsync(cacheKey, result, _cacheTtl);

            _logger.LogInformation("Successfully calculated and cached stats for User {UserId} ({Year}-{Month}).", userId, year, month);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "System error while calculating statistics for user {UserId}.", userId);
            return null;
        }
    }
}
using DiaryApp.Application.Interfaces;
using DiaryApp.Infrastructure.Data;
using Google.Cloud.Firestore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiaryApp.Infrastructure.Workers;

public class StreakCleanupWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StreakCleanupWorker> _logger;

    public StreakCleanupWorker(IServiceProvider serviceProvider, ILogger<StreakCleanupWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Streak Cleanup Worker is starting...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var nextRun = DateTime.UtcNow.Date.AddDays(1); 
                var delay = nextRun - DateTime.UtcNow;

                _logger.LogInformation("Next scan will execute in: {Delay}", delay);
                
                // Dòng 35 đã được bọc an toàn trong try-catch lớn
                await Task.Delay(delay, stoppingToken);

                try
                {
                    await ResetExpiredStreaks();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while executing end-of-day streak reset.");
                }
            }
        }
        // THÊM ĐOẠN NÀY: Bắt lỗi khi hệ thống yêu cầu Worker dừng ngủ để tắt Server
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Streak Cleanup Worker is stopping gracefully because the server is shutting down.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "A fatal error occurred in Streak Cleanup Worker.");
        }
    }

    private async Task ResetExpiredStreaks()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FirestoreProvider>().Database;
        
        // yesterday
        var threshold = DateTime.UtcNow.Date.AddDays(-1);
        
        // lastlogdate < threshold && curent streak > 0
        var query = db.Collection("userStreaks")
            .WhereLessThan("LastLogDate", Timestamp.FromDateTime(threshold.ToUniversalTime()))
            .WhereGreaterThan("CurrentStreak", 0);

        var snapshot = await query.GetSnapshotAsync();
        
        if (snapshot.Documents.Count == 0) return;

        var batch = db.StartBatch();
        int count = 0;

        foreach (var doc in snapshot.Documents)
        {
            batch.Update(doc.Reference, new Dictionary<string, object>
            {
                { "CurrentStreak", 0 },
                { "UpdatedAt", Timestamp.GetCurrentTimestamp() }
            });
            count++;
        }

        await batch.CommitAsync();
        
        _logger.LogInformation("Successfully reset streaks for {Count} inactive users.", count);
    }
}
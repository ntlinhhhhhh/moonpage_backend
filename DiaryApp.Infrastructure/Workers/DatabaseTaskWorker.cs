using System.Text;
using System.Text.Json;
using DiaryApp.Application.DTOs.Queue;
using DiaryApp.Application.Interfaces;
using DiaryApp.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DiaryApp.Infrastructure.Workers;

public class DatabaseTaskWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseTaskWorker> _logger;
    private readonly string _rabbitMqUrl;

    public DatabaseTaskWorker(IServiceProvider serviceProvider, IConfiguration config, ILogger<DatabaseTaskWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitMqUrl = config["RabbitMQSettings:Url"]!;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_rabbitMqUrl) };
        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(null, stoppingToken);

        await channel.QueueDeclareAsync("db_tasks_queue", true, false, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var data = JsonSerializer.Deserialize<DatabaseTaskPayload>(message);

                if (data != null)
                {
                    using var scope = _serviceProvider.CreateScope();

                    switch (data.TaskType)
                    {
                        case DbTaskType.LinkMomentsToLog:
                        {
                            var momentRepo = scope.ServiceProvider.GetRequiredService<IMomentRepository>();
                            await momentRepo.LinkMomentsToLogAsync(data.UserId, data.DateStr, data.EntityId);
                            _logger.LogInformation("Successfully linked Moments to DailyLog: {LogId} for User: {UserId}", data.EntityId, data.UserId);
                            break;
                        }

                        case DbTaskType.ProcessRewards:
                        {
                            var streakRepo = scope.ServiceProvider.GetRequiredService<IUserStreakRepository>();
                            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                            
                            var streak = await streakRepo.GetByUserIdAsync(data.UserId) 
                                        ?? new UserStreak { UserId = data.UserId };

                            if (!DateTime.TryParse(data.DateStr, out DateTime logDate)) 
                            {
                                logDate = DateTime.UtcNow.Date;
                            }
                            
                            var today = logDate.Date;
                            var lastLog = streak.LastLogDate?.Date;

                            if (lastLog == today) return; 

                            int coinBonus = 10;

                            if (lastLog == today.AddDays(-1)) 
                            {
                                streak.CurrentStreak++;
                            } 
                            else 
                            {
                                if (streak.StreakFreezes > 0 && lastLog == today.AddDays(-2)) 
                                {
                                    streak.StreakFreezes--;
                                    streak.CurrentStreak += 1;
                                    _logger.LogInformation("User {UserId} streak saved using a Streak Freeze!", data.UserId);
                                } 
                                else 
                                {
                                    streak.CurrentStreak = 1;
                                    _logger.LogInformation("User {UserId} streak broken. Reset to 1.", data.UserId);
                                }
                            }

                            if (streak.CurrentStreak > streak.LongestStreak)   
                                streak.LongestStreak = streak.CurrentStreak;

                            if (streak.CurrentStreak == 7) coinBonus += 25;
                            else if (streak.CurrentStreak == 15) coinBonus += 50;
                            
                            if (streak.CurrentStreak > 0 && streak.CurrentStreak % 30 == 0)
                            {
                                coinBonus += 100;
                                streak.StreakFreezes += 1;
                                _logger.LogInformation("User {UserId} received 1 Streak Freeze for reaching {Streak} days streak!", data.UserId, streak.CurrentStreak);
                            }

                            streak.LastLogDate = today;

                            await Task.WhenAll(
                                streakRepo.UpsertAsync(streak),
                                userRepo.UpdateCoinBalanceAsync(data.UserId, coinBonus)
                            );
                            
                            _logger.LogInformation("Successfully processed rewards for User {UserId} at {DateStr}.", data.UserId, data.DateStr);
                            break;
                        }

                        case DbTaskType.SyncUserMedia:
                        {
                            var momentRepo = scope.ServiceProvider.GetRequiredService<IMomentRepository>();
                            await momentRepo.SyncUserMediaInMomentsAsync(data.UserId, data.UserName ?? string.Empty, data.AvatarUrl ?? string.Empty);
                            _logger.LogInformation("Successfully synced User Media in Moments for User: {UserId}", data.UserId);
                            break;
                        }
                    }
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing Background Database Task!");
                await channel.BasicNackAsync(ea.DeliveryTag, false, true); 
            }
        };

        await channel.BasicConsumeAsync("db_tasks_queue", false, consumer);
        
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
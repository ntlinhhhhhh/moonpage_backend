using System.Text;
using System.Text.Json;
using DiaryApp.Application.Interfaces;
using DiaryApp.Application.Interfaces.Services;
using DiaryApp.Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DiaryApp.Infrastructure.Workers;

public class ImageUploadWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ImageUploadWorker> _logger;
    private readonly string _rabbitMqUrl;

    public ImageUploadWorker(IServiceProvider serviceProvider, IOptions<RabbitMQSettings> options, ILogger<ImageUploadWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitMqUrl = options.Value.Url;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrEmpty(_rabbitMqUrl))
        {
            _logger.LogCritical("RabbitMQ Connection URL is missing! Background worker cannot start.");
            return;
        }

        var factory = new ConnectionFactory { Uri = new Uri(_rabbitMqUrl) };
        IConnection? connection = null;
        IChannel? channel = null;

        int retryCount = 0;
        const int maxRetries = 10;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Attempting to connect to RabbitMQ (Attempt {Count}/{Max})...", retryCount + 1, maxRetries);
                connection = await factory.CreateConnectionAsync(stoppingToken);
                channel = await connection.CreateChannelAsync(null, stoppingToken);
                _logger.LogInformation("Successfully connected to RabbitMQ.");
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    _logger.LogCritical(ex, "Could not connect to RabbitMQ after {Max} attempts. Worker stopping.", maxRetries);
                    return;
                }
                _logger.LogWarning("RabbitMQ connection failed. Retrying in 5s... ({Message})", ex.Message);
                await Task.Delay(5000, stoppingToken);
            }
        }

        if (connection == null || channel == null) return;

        await using (connection)
        await using (channel)
        {
            await channel.QueueDeclareAsync("image_upload_queue", true, false, false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var data = JsonSerializer.Deserialize<ImageUploadPayload>(message);

                if (data != null && File.Exists(data.TempImagePath))
                {
                    using var scope = _serviceProvider.CreateScope();
                    var storageService = scope.ServiceProvider.GetRequiredService<IGoogleStorageService>();
                    
                    try
                    {
                        using var stream = File.OpenRead(data.TempImagePath);
                        var folder = data.UploadType switch
                        {
                            ImageUploadType.DailyLog => "dailylogs",
                            ImageUploadType.Avatar => "avatars",
                            ImageUploadType.Moment => "moments",
                            _ => "others"
                        };

                        var fileName = Path.GetFileName(data.TempImagePath);
                        var imageUrl = await storageService.UploadImageAsync(stream, fileName, folder);

                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            if (data.UploadType == ImageUploadType.Moment)
                            {
                                var momentService = scope.ServiceProvider.GetRequiredService<IMomentService>();
                                await momentService.UpdateImageUrlAsync(data.EntityId, imageUrl);
                            }
                            else if (data.UploadType == ImageUploadType.DailyLog)
                            {
                                var logService = scope.ServiceProvider.GetRequiredService<IDailyLogService>();
                                await logService.AddPhotoToLogAsync(data.UserId, data.EntityId, imageUrl);
                            } else if (data.UploadType == ImageUploadType.Avatar)
                            {
                                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                                await userService.UpdateAvatarUrlAsync(data.UserId, imageUrl);
                            }
                        }

                        File.Delete(data.TempImagePath);
                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Background processing error in ReceivedAsync");
                        await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                }
            };

            await channel.BasicConsumeAsync("image_upload_queue", false, consumer, cancellationToken: stoppingToken);
            
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("ImageUploadWorker is stopping...");
            }
        }
    }

    private record ImagePayload(string MomentId, string UserId, string TempPath, string FileName);
}
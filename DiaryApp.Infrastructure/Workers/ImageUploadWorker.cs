using System.Text;
using System.Text.Json;
using DiaryApp.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DiaryApp.Infrastructure.Workers;

public class ImageUploadWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ImageUploadWorker> _logger;
    private readonly string _rabbitMqUrl;

    public ImageUploadWorker(IServiceProvider serviceProvider, IConfiguration config, ILogger<ImageUploadWorker> logger)
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

        await channel.QueueDeclareAsync("image_upload_queue", true, false, false);

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
                    var folder = data.UploadType == ImageUploadType.DailyLog ? "dailylogs" : "moments";
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

        await channel.BasicConsumeAsync("image_upload_queue", false, consumer);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private record ImagePayload(string MomentId, string UserId, string TempPath, string FileName);
}
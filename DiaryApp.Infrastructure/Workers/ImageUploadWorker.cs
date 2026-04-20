using System.Text;
using System.Text.Json;
using DiaryApp.Application.DTOs.Moment;
using DiaryApp.Application.Interfaces.Services;
using DiaryApp.Infrastructure.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DiaryApp.Infrastructure.Workers;

public class ImageUploadWorker(
    IServiceProvider serviceProvider,
    IConfiguration configuration) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly string _rabbitMqUrl = configuration["RabbitMQSettings:Url"] ?? string.Empty;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_rabbitMqUrl))
        {
            Console.WriteLine("WARNING: RabbitMQ URL not found in configuration! ImageUploadWorker is stopping.");
            return; 
        }

        try
        {
            var factory = new ConnectionFactory { Uri = new Uri(_rabbitMqUrl) };
            var connection = await factory.CreateConnectionAsync(stoppingToken);
            var channel = await connection.CreateChannelAsync(null, stoppingToken);

            await channel.QueueDeclareAsync(queue: "image_upload_queue", durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var data = JsonSerializer.Deserialize<MomentMessagePayload>(message);

                    if (data == null)
                    {
                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                        return;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    var cloudinaryService = scope.ServiceProvider.GetRequiredService<ICloudinaryService>();
                    var momentService = scope.ServiceProvider.GetRequiredService<IMomentService>();

                    using var stream = File.OpenRead(data.TempImagePath);
                    string? imageUrl = await cloudinaryService.UploadImageAsync(stream, $"moment_{Guid.NewGuid()}.png", "moments");

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        var requestDto = new MomentRequestDto
                        {
                            DailyLogId = data.DailyLogId,
                            Caption = data.Caption,
                            IsPublic = data.IsPublic,
                            CapturedAt = data.CapturedAt
                        };

                        await momentService.CreateMomentAsync(data.UserId, requestDto, imageUrl);
                        Console.WriteLine($"Created moment successfully for user {data.UserId}");
                    }

                    if (File.Exists(data.TempImagePath))
                    {
                        File.Delete(data.TempImagePath);
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Background processing error in ReceivedAsync: {ex.Message}");
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await channel.BasicConsumeAsync(queue: "image_upload_queue", autoAck: false, consumer: consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing RabbitMQ connection: {ex.Message}");
        }
    }

    private class MomentMessagePayload
    {
        public string UserId { get; set; } = string.Empty;
        public string DailyLogId { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CapturedAt { get; set; }
        public string TempImagePath { get; set; } = string.Empty;
    }
}
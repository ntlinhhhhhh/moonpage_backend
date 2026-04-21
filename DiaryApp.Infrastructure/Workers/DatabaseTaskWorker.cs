using System.Text;
using System.Text.Json;
using DiaryApp.Application.DTOs.Queue;
using DiaryApp.Application.Interfaces;
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

        // Khai báo hàng đợi riêng cho DB Tasks
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
                    // Tạo một Scope mới vì Repository thường là Scoped Service
                    using var scope = _serviceProvider.CreateScope();

                    // Điều hướng xử lý dựa trên TaskType
                    switch (data.TaskType)
                    {
                        case DbTaskType.LinkMomentsToLog:
                            var momentRepo = scope.ServiceProvider.GetRequiredService<IMomentRepository>();
                            await momentRepo.LinkMomentsToLogAsync(data.UserId, data.DateStr, data.EntityId);
                            _logger.LogInformation("Đã tự động liên kết các Moments vào DailyLog: {LogId} cho User: {UserId}", data.EntityId, data.UserId);
                            break;
                            
                        // Thêm các case khác ở đây trong tương lai
                    }
                }

                // Báo cáo cho RabbitMQ biết là đã xử lý xong tin nhắn thành công
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý Background Database Task!");
                // Nếu lỗi do code hoặc DB, Nack để trả tin nhắn về Queue (tùy thuộc chiến lược retry của bạn)
                // Tham số cuối là 'requeue: true' - tin nhắn sẽ được đẩy lại vào hàng đợi
                await channel.BasicNackAsync(ea.DeliveryTag, false, true); 
            }
        };

        await channel.BasicConsumeAsync("db_tasks_queue", false, consumer);
        
        // Giữ cho Worker luôn sống và lắng nghe
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
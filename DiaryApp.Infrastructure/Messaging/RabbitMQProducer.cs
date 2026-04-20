using System.Text;
using System.Text.Json;
using DiaryApp.Application.Interfaces;
using DiaryApp.Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DiaryApp.Infrastructure.Messaging;

public class RabbitMQProducer(IConfiguration configuration) : IMessageProducer
{
    private readonly string _rabbitMqUrl = configuration["RabbitMQSettings:Url"] ?? string.Empty;

    public async Task SendMessageAsync<T>(T message, string queueName)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_rabbitMqUrl) };
        
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: queueName, 
            durable: true, 
            exclusive: false, 
            autoDelete: false, 
            arguments: null
        );

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: string.Empty, 
            routingKey: queueName, 
            body: body
        );
    }
}
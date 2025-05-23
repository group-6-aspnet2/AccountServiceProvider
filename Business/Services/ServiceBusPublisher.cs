using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace Business.Services;

// Tagit hjälp av chatgpt
public class ServiceBusPublisher
{
    private readonly string _connectionString;
    private readonly string _queueName;

    public ServiceBusPublisher(IConfiguration config)
    {
        _connectionString = config["ServiceBus:ConnectionString"]!;
        _queueName = config["ServiceBus:QueueName"]!;
    }

    public async Task SendMessageAsync(string messageBody)
    {
        var client = new ServiceBusClient(_connectionString);
        var sender = client.CreateSender(_queueName);

        var message = new ServiceBusMessage(messageBody);
        await sender.SendMessageAsync(message);

        await sender.DisposeAsync();
        await client.DisposeAsync();
    }
}

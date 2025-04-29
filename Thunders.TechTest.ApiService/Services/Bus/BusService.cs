using RabbitMQ.Client;
using System.Text.Json;
using Thunders.TechTest.ApiService.Models.Bus;

namespace Thunders.TechTest.ApiService.Services.Bus;

public class BusService(IConnection messageConnection) : IBusService
{
    private const string QueueName = "Thunders.TechTest";

    public Task PublishMessageAsync(ReportBusModel model)
    {
        using var channel = messageConnection.CreateModel();
        channel.QueueDeclare(QueueName, exclusive: false);
        channel.BasicPublish(
            exchange: "",
            routingKey: QueueName,
            basicProperties: null,
            body: JsonSerializer.SerializeToUtf8Bytes(model));

        return Task.CompletedTask;
    }
}

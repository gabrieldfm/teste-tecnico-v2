using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using Thunders.TechTest.ApiService.Models.Bus;
using Thunders.TechTest.ApiService.Services.Toll;

namespace Thunders.TechTest.ApiService.Worker;

public class MessageConsumer(IServiceProvider serviceProvider) : BackgroundService
{
    private IConnection? _messageConnection;
    private IModel? _messageChannel;
    private const string QueueName = "Thunders.TechTest";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _messageConnection = serviceProvider.GetService<IConnection>();
        _messageChannel = _messageConnection!.CreateModel();
        _messageChannel.QueueDeclare(QueueName, exclusive: false);

        var consumer = new EventingBasicConsumer(_messageChannel);
        consumer.Received += async(ch, ea) =>
        {
            await ProcessMessageAsync(ch, ea);
            await Task.CompletedTask;
        };

        _messageChannel.BasicConsume(queue: QueueName,
                                     autoAck: true,
                                     consumer: consumer);

        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        _messageChannel?.Dispose();
    }

    private async Task ProcessMessageAsync(object? sender, BasicDeliverEventArgs args)
    {
        var body = args.Body;

        var model = JsonSerializer.Deserialize<ReportBusModel>(body.Span);
        if (model is null)
            return;

        using (var scope = serviceProvider.CreateScope()) 
        {
            var tollService = scope.ServiceProvider.GetService<ITollService>()!;
            switch (model.ReportType)
            {
                case "GetTotalRevenueByHourAndCity":
                    await tollService.CreateReportTotalRevenueByHourAndCityAsync(model);
                    break;
                case "GetVeichleByToll":
                    await tollService.CreateReportGetVeichleByTollAsync(model);
                    break;
                case "GetTopRevenueByMonth":
                    await tollService.CreateReportGetTopRevenueByMonthAsync(model);
                    break;
                default:
                    return;
            }
        }            
    }
}

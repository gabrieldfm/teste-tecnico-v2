using Thunders.TechTest.ApiService.Models.Bus;

namespace Thunders.TechTest.ApiService.Services.Bus;

public interface IBusService
{
    Task PublishMessageAsync(ReportBusModel model);
}

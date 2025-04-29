using Thunders.TechTest.ApiService.Models.Bus;
using Thunders.TechTest.ApiService.Models.Requests;
using Thunders.TechTest.ApiService.Models.Responses;

namespace Thunders.TechTest.ApiService.Services.Toll;

public interface ITollService
{
    Task<BaseReponseModel<CreateTollUsageResponseModel>> RegisterUsageAsync(CreateTollUsageRequestModel request);
    Task<BaseReponseModel<GetTollUsageReportResponseModel>> GetTotalRevenueByHourAndCityAsync();
    Task<BaseReponseModel<GetTollUsageReportResponseModel>> GetVeichleByTollAsync();
    Task<BaseReponseModel<GetTollUsageReportResponseModel>> GetTopRevenueByMonthAsync(GetTopRevenueByMonthRequestModel request);
    Task CreateReportTotalRevenueByHourAndCityAsync(ReportBusModel model);
    Task CreateReportGetVeichleByTollAsync(ReportBusModel model);
    Task CreateReportGetTopRevenueByMonthAsync(ReportBusModel model);
}

using Thunders.TechTest.ApiService.Models.Responses;

namespace Thunders.TechTest.ApiService.Services.Report;

public interface IReportService
{
    Task<BaseReponseModel<GetReportResponseModel>> GetReportAsync(Guid id);
}

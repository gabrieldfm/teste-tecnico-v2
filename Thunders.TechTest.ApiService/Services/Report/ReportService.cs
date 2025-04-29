using StackExchange.Redis;
using Thunders.TechTest.ApiService.Models.Responses;
using System.Text;

namespace Thunders.TechTest.ApiService.Services.Report;

public class ReportService(IConnectionMultiplexer redisClient) : IReportService
{
    public async Task<BaseReponseModel<GetReportResponseModel>> GetReportAsync(Guid id)
    {
        var cache = redisClient.GetDatabase();
        var data = await cache.StringGetAsync(id.ToString());
        if (!data.HasValue)
        {
            return new BaseReponseModel<GetReportResponseModel>
            {
                HasError = true,
                Message = "report not found"
            };            
        }

        var dataValue = data.ToString();
        if (string.Equals("requested", dataValue, StringComparison.OrdinalIgnoreCase)) 
        {
            return new BaseReponseModel<GetReportResponseModel>
            {
                HasError = false,
                Message = "report not processed yet"
            };
        }

        if (string.Equals("empty", dataValue, StringComparison.OrdinalIgnoreCase))
        {
            return new BaseReponseModel<GetReportResponseModel>
            {
                HasError = false,
                Message = "no data to show"
            };
        }

        return new BaseReponseModel<GetReportResponseModel>
        {
            HasError = false,
            Message = "Success",
            ResponseData = new GetReportResponseModel
            {
                FileName = CreateFileName(id),
                FileStream = Encoding.UTF8.GetBytes(dataValue),
                ContentType = "application/json",
            }
        };
    }

    private static string CreateFileName(Guid id)
    {
        return id.ToString() + "_" + DateTime.UtcNow.ToString() + ".json";
    }
}

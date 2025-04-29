using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Thunders.TechTest.ApiService.Infra.ApplicationDbContext;
using Thunders.TechTest.ApiService.Models.Bus;
using Thunders.TechTest.ApiService.Models.Mappings;
using Thunders.TechTest.ApiService.Models.Requests;
using Thunders.TechTest.ApiService.Models.Responses;
using Thunders.TechTest.ApiService.Services.Bus;
using StackExchange.Redis;

namespace Thunders.TechTest.ApiService.Services.Toll;

public class TollService(AppDbContext appDbContext, IBusService bus, ILogger<TollService> logger, IConnectionMultiplexer redisClient) : ITollService
{
    public async Task<BaseReponseModel<CreateTollUsageResponseModel>> RegisterUsageAsync(CreateTollUsageRequestModel request)
    {
        if (request.Value <= 0)
        {
            return CreateErrorResponse<CreateTollUsageResponseModel>("Invalid value");
        }

        if (string.IsNullOrWhiteSpace(request.City) ||
            string.IsNullOrWhiteSpace(request.State) || 
            string.IsNullOrWhiteSpace(request.TollDescription)) 
        {
            return CreateErrorResponse<CreateTollUsageResponseModel>("Invalid toll information");
        }

        logger.LogInformation(message: "start to add record");

        var tollUsage = request.ToDomain();
        tollUsage.SetUsageDate();

        await appDbContext.TollUsages.AddAsync(tollUsage);
        await appDbContext.SaveChangesAsync();

        logger.LogInformation(message: "record saved");

        return CreateSuccessResponse(tollUsage.ToServiceModel());
    }

    public async Task<BaseReponseModel<GetTollUsageReportResponseModel>> GetTopRevenueByMonthAsync(GetTopRevenueByMonthRequestModel request)
    {
        var reportId = Guid.NewGuid();
        var reportObject = new ReportBusModel
        {
            ReportType = "GetTopRevenueByMonth",
            NumberOfRecords = request.NumberOfRecords,
            BaseMonth = request.BaseMonth ?? DateTime.UtcNow.Month,
            Id = reportId.ToString(),
        };

        await SaveAndPublishAsync(reportId, reportObject);

        return CreateSuccessResponse(new GetTollUsageReportResponseModel { ReportId = reportId });
    }    

    public async Task<BaseReponseModel<GetTollUsageReportResponseModel>> GetTotalRevenueByHourAndCityAsync()
    {
        var reportId = Guid.NewGuid();
        var reportObject = new ReportBusModel
        {
            ReportType = "GetTotalRevenueByHourAndCity",
            Id = reportId.ToString(),
        };

        await SaveAndPublishAsync(reportId, reportObject);

        return CreateSuccessResponse(new GetTollUsageReportResponseModel { ReportId = reportId });
    }

    public async Task<BaseReponseModel<GetTollUsageReportResponseModel>> GetVeichleByTollAsync()
    {
        var reportId = Guid.NewGuid();
        var reportObject = new ReportBusModel
        {
            ReportType = "GetVeichleByToll",
            Id = reportId.ToString(),
        };

        await SaveAndPublishAsync(reportId, reportObject);

        return CreateSuccessResponse(new GetTollUsageReportResponseModel { ReportId = reportId });
    }

    public async Task CreateReportTotalRevenueByHourAndCityAsync(ReportBusModel model)
    {
        try
        {
            var data = await appDbContext.TollUsages
                .AsNoTracking()
                .GroupBy(r => r.City)
                .ToDictionaryAsync(
                    cg => cg.Key,
                    cg => cg.GroupBy(r => r.CreatedDate.Hour)
                                .ToDictionary(
                                    hg => new TimeOnly(hg.Key, 0, 0),
                                    hg => hg.Sum(r => r.Value) 
                                )
                );

            var cache = redisClient.GetDatabase();
            if (data is null || !data.Any())
            {
                await cache.StringSetAsync(model.Id.ToString(), "empty", TimeSpan.FromDays(5));
                return;
            }

            await cache.StringSetAsync(model.Id.ToString(), JsonSerializer.Serialize(data), TimeSpan.FromDays(5));
        }
        catch (Exception ex)
        {
            logger.LogError($"error generating report {model.Id.ToString()}: {ex.Message}");
            throw;
        }
    }

    public async Task CreateReportGetVeichleByTollAsync(ReportBusModel model)
    {
        try
        {
            var data = await appDbContext.TollUsages
                .AsNoTracking()
                .GroupBy(r => r.TollDescription)
                .ToDictionaryAsync(
                    toll => toll.Key,
                    toll => toll.GroupBy(r => r.VehicleType)
                                .ToDictionary(
                                    vc => vc.Key,
                                    vc => vc.Count()
                                )
                );

            var cache = redisClient.GetDatabase();
            if (data is null || !data.Any())
            {
                await cache.StringSetAsync(model.Id.ToString(), "empty", TimeSpan.FromDays(5));
                return;
            }

            await cache.StringSetAsync(model.Id.ToString(), JsonSerializer.Serialize(data), TimeSpan.FromDays(5));
        }
        catch (Exception ex)
        {
            logger.LogError($"error generating report {model.Id.ToString()}: {ex.Message}");
            throw;
        }
    }

    public async Task CreateReportGetTopRevenueByMonthAsync(ReportBusModel model)
    {
        try
        {
            var data = await appDbContext.TollUsages
                .AsNoTracking()
                .Where(r => r.CreatedDate.Month == model.BaseMonth)
                .GroupBy(r => r.TollDescription)
                .Select(group => new
                {
                    Toll = group.Key,
                    TotalValue = group.Sum(r => r.Value)
                })
                .OrderByDescending(x => x.TotalValue)
                .Take(model.NumberOfRecords ?? 10)
                .Select(x => new { x.Toll, x.TotalValue })
                .ToListAsync();

            var cache = redisClient.GetDatabase();
            if (data is null || !data.Any())
            {
                await cache.StringSetAsync(model.Id.ToString(), "empty", TimeSpan.FromDays(5));
                return;
            }

            await cache.StringSetAsync(model.Id.ToString(), JsonSerializer.Serialize(data), TimeSpan.FromDays(5));
        }
        catch (Exception ex)
        {
            logger.LogError($"error generating report {model.Id.ToString()}: {ex.Message}");
            throw;
        }
    }

    private async Task SaveAndPublishAsync(Guid reportId, ReportBusModel reportObject)
    {
        var cache = redisClient.GetDatabase();
        await cache.StringSetAsync(reportId.ToString(), "requested", TimeSpan.FromDays(5));

        logger.LogInformation($"report {reportId.ToString()} requested");

        await bus.PublishMessageAsync(reportObject);
    }

    private static BaseReponseModel<T> CreateSuccessResponse<T>(T response)
    {
        return new BaseReponseModel<T>
        {
            HasError = false,
            Message = "Success",
            ResponseData = response
        };
    }

    private static BaseReponseModel<T> CreateErrorResponse<T>(string message)
    {
        return new BaseReponseModel<T>
        {
            HasError = true,
            Message = message
        };
    }    
}

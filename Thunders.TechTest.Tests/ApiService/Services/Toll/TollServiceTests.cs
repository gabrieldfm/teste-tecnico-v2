using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using StackExchange.Redis;
using Thunders.TechTest.ApiService.Domain.Entities;
using Thunders.TechTest.ApiService.Domain.Enumerations;
using Thunders.TechTest.ApiService.Infra.ApplicationDbContext;
using Thunders.TechTest.ApiService.Models.Bus;
using Thunders.TechTest.ApiService.Models.Requests;
using Thunders.TechTest.ApiService.Services.Bus;
using Thunders.TechTest.ApiService.Services.Toll;

namespace Thunders.TechTest.Tests.ApiService.Services.Toll;

public class TollServiceTests
{
    private readonly ITollService _tollService;
    private readonly IConnectionMultiplexer _redisClient;
    private readonly IDatabase _redisDb;
    private readonly IBusService _busService;
    private readonly AppDbContext _appDbContext;
    private readonly ILogger<TollService> _logger;

    public TollServiceTests()
    {
        _redisClient = Substitute.For<IConnectionMultiplexer>();
        _redisDb = Substitute.For<IDatabase>();
        _logger = Substitute.For<ILogger<TollService>>();
        _busService = Substitute.For<IBusService> ();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDatabase")
            .Options;
        _appDbContext = new AppDbContext(options);
        _appDbContext.Database.EnsureCreated();

        _tollService = new TollService(_appDbContext, _busService, _logger, _redisClient);
    }

    [Fact]
    public async Task RegisterUsageAsync_InvalidValue_ReturnsError()
    {
        // Arrange
        var request = new CreateTollUsageRequestModel { Value = 0 };

        // Act
        var result = await _tollService.RegisterUsageAsync(request);

        // Assert
        Assert.True(result.HasError);
        Assert.Equal("Invalid value", result.Message);
    }

    [Theory]
    [InlineData("", "SC", "praça-palhoça")]
    [InlineData("palhoça", "", "praça-palhoça")]
    [InlineData("palhoça", "SC", "")]
    public async Task RegisterUsageAsync_InvalidTollInfo_ReturnsError(string city, string state, string description)
    {
        // Arrange
        var request = new CreateTollUsageRequestModel
        {
            Value = 1.5m,
            City = city,
            State = state,
            TollDescription = description
        };

        // Act
        var result = await _tollService.RegisterUsageAsync(request);

        // Assert
        Assert.True(result.HasError);
        Assert.Equal("Invalid toll information", result.Message);
    }

    [Fact(Skip = "fix tests with db")]
    public async Task RegisterUsageAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new CreateTollUsageRequestModel
        {
            Value = 1.5m,
            City = "palhoça",
            State = "SC",
            TollDescription = "praça-palhoça",
            VehicleType = VehicleType.Car
        };

        // Act
        var result = await _tollService.RegisterUsageAsync(request);

        // Assert
        Assert.False(result.HasError);
        Assert.Equal("Success", result.Message);
        Assert.NotNull(result.ResponseData);
        await _appDbContext.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task GetTopRevenueByMonthAsync_ReturnsReportId()
    {
        // Arrange
        var request = new GetTopRevenueByMonthRequestModel { NumberOfRecords = 5, BaseMonth = 3 };
        _redisClient.GetDatabase().Returns(_redisDb);
        _redisDb.StringSetAsync(Arg.Any<RedisKey>(),
            "requested",
            TimeSpan.FromDays(5))
            .Returns(true);

        // Act
        var result = await _tollService.GetTopRevenueByMonthAsync(request);

        // Assert
        Assert.False(result.HasError);
        Assert.Equal("Success", result.Message);
        await _redisDb.Received(1).StringSetAsync(
            Arg.Any<RedisKey>(),
            "requested",
            TimeSpan.FromDays(5)
        );
        await _busService.Received(1).PublishMessageAsync(Arg.Any<ReportBusModel>());
    }

    [Fact]
    public async Task GetTotalRevenueByHourAndCityAsync_ReturnsReportId()
    {
        // Arrange
        _redisClient.GetDatabase().Returns(_redisDb);
        _redisDb.StringSetAsync(Arg.Any<RedisKey>(),
            "requested",
            TimeSpan.FromDays(5))
            .Returns(true);


        // Act
        var result = await _tollService.GetTotalRevenueByHourAndCityAsync();

        // Assert
        Assert.False(result.HasError);
        Assert.Equal("Success", result.Message);
        await _redisDb.Received(1).StringSetAsync(
            Arg.Any<RedisKey>(),
            "requested",
            TimeSpan.FromDays(5)
        );
        await _busService.Received(1).PublishMessageAsync(Arg.Any<ReportBusModel>());
    }

    [Fact]
    public async Task GetVeichleByTollAsync_ReturnsReportId()
    {
        // Arrange
        _redisClient.GetDatabase().Returns(_redisDb);
        _redisDb.StringSetAsync(Arg.Any<RedisKey>(),
            "requested",
            TimeSpan.FromDays(5))
            .Returns(true);


        // Act
        var result = await _tollService.GetVeichleByTollAsync();

        // Assert
        Assert.False(result.HasError);
        Assert.Equal("Success", result.Message);
        await _redisDb.Received(1).StringSetAsync(
            Arg.Any<RedisKey>(),
            "requested",
            TimeSpan.FromDays(5)
        );
        await _busService.Received(1).PublishMessageAsync(Arg.Any<ReportBusModel>());
    }

    [Fact(Skip = "fix tests with db")]
    public async Task CreateReportTotalRevenueByHourAndCityAsync_WithData_CachesReport()
    {
        // Arrange
        var reportId = Guid.NewGuid().ToString();
        var model = new ReportBusModel { Id = reportId, ReportType = "GetTotalRevenueByHourAndCity" };
        var dataItem = new List<TollUsage>
        {
            new TollUsage { TollDescription = "Palhoça", VehicleType = VehicleType.Car, State = "SC", City = "Palhoça", Value = 10 },
            new TollUsage { TollDescription = "Palhoça", VehicleType = VehicleType.Car, State = "SC", City = "Palhoça", Value = 15 },
            new TollUsage { TollDescription = "Florianópolis", VehicleType = VehicleType.Car, State = "SC", City = "Florianópolis", Value = 20},
        };
        foreach (var item in dataItem)
        {
            item.SetUsageDate();
        }

        // Adicionar dados de pedágio para o teste
        await _appDbContext.TollUsages.AddRangeAsync(dataItem);

        
        await _appDbContext.SaveChangesAsync();

        // Act
        await _tollService.CreateReportTotalRevenueByHourAndCityAsync(model);

        // Assert
        await _redisDb.Received(1).StringSetAsync(
            reportId,
            Arg.Is<string>(s => !string.IsNullOrEmpty(s) && s != "empty"), // Verifica se o valor não é nulo ou "empty"
            Arg.Any<TimeSpan>()
        );
    }
}

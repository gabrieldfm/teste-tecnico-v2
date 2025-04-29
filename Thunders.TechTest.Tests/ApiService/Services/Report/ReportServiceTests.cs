using NSubstitute;
using StackExchange.Redis;
using System.Text;
using Thunders.TechTest.ApiService.Services.Report;

namespace Thunders.TechTest.Tests.ApiService.Services.Report;

public class ReportServiceTests
{
    private readonly IReportService _reportService;
    private readonly IConnectionMultiplexer _redisClient;
    private readonly IDatabase _redisDb;

	public ReportServiceTests()
	{
        _redisClient = Substitute.For<IConnectionMultiplexer>();
        _redisDb = Substitute.For<IDatabase>();

        _reportService = new ReportService(_redisClient);
    }

    [Fact]
    public async Task GetReportAsync_ReportNotFound_ReturnsError()
    {
        // Arrange
        var redisData = RedisValue.Null;
        var reportId = Guid.NewGuid();
        _redisClient.GetDatabase().Returns(_redisDb);
        _redisDb.StringGetAsync(reportId.ToString())
            .Returns(redisData);        

        // Act
        var result = await _reportService.GetReportAsync(reportId);

        // Assert
        Assert.True(result.HasError);
        Assert.Equal("report not found", result.Message);
        Assert.Null(result.ResponseData);
    }

    [Fact]
    public async Task GetReportAsync_ReportRequested_ReturnsNotProcessed()
    {
        // Arrange
        var redisData = "requested";
        var reportId = Guid.NewGuid();
        _redisClient.GetDatabase().Returns(_redisDb);
        _redisDb.StringGetAsync(reportId.ToString())
            .Returns(redisData);

        // Act
        var result = await _reportService.GetReportAsync(reportId);

        // Assert
        Assert.False(result.HasError);
        Assert.Equal("report not processed yet", result.Message);
        Assert.Null(result.ResponseData);
    }

    [Fact]
    public async Task GetReportAsync_EmptyData_ReturnsNoData()
    {
        // Arrange
        var redisData = "empty";
        var reportId = Guid.NewGuid();
        _redisClient.GetDatabase().Returns(_redisDb);
        _redisDb.StringGetAsync(reportId.ToString())
            .Returns(redisData);

        // Act
        var result = await _reportService.GetReportAsync(reportId);

        // Assert
        Assert.False(result.HasError);
        Assert.Equal("no data to show", result.Message);
        Assert.Null(result.ResponseData);
    }

    [Fact]
    public async Task GetReportAsync_ReportSuccess_ReturnsReportData()
    {
        // Arrange
        var reportData = "{\"name\":\"Test Report\"}";
        var reportId = Guid.NewGuid();
        _redisClient.GetDatabase().Returns(_redisDb);
        _redisDb.StringGetAsync(reportId.ToString())
            .Returns(reportData);

        // Act
        var result = await _reportService.GetReportAsync(reportId);

        // Assert
        Assert.False(result.HasError);
        Assert.Equal("Success", result.Message);
        Assert.NotNull(result.ResponseData);
        Assert.Equal($"{reportId}_{DateTime.UtcNow.ToString()}.json", result.ResponseData.FileName);
        Assert.Equal(Encoding.UTF8.GetBytes(reportData), result.ResponseData.FileStream);
        Assert.Equal("application/json", result.ResponseData.ContentType);
    }
}

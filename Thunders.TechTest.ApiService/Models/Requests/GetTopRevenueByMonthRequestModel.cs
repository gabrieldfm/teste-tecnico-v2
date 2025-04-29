namespace Thunders.TechTest.ApiService.Models.Requests;

public class GetTopRevenueByMonthRequestModel
{
    public int NumberOfRecords { get; set; } = 10;
    public int? BaseMonth { get; set; }
}

using Thunders.TechTest.ApiService.Domain.Enumerations;

namespace Thunders.TechTest.ApiService.Models.Requests;

public class CreateTollUsageRequestModel
{
    public string TollDescription { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public decimal Value { get; set; }
    public VehicleType VehicleType { get; set; }
}

using System.Diagnostics.CodeAnalysis;
using Thunders.TechTest.ApiService.Domain.Enumerations;

namespace Thunders.TechTest.ApiService.Models.Responses;

[ExcludeFromCodeCoverage]
public class CreateTollUsageResponseModel
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string TollDescription { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public decimal Value { get; set; }
    public VehicleType VehicleType { get; set; }
}

using Thunders.TechTest.ApiService.Domain.Entities;
using Thunders.TechTest.ApiService.Models.Requests;
using Thunders.TechTest.ApiService.Models.Responses;

namespace Thunders.TechTest.ApiService.Models.Mappings;

public static class TollUsageMappingExtension
{
    public static TollUsage ToDomain(this CreateTollUsageRequestModel model)
    {
        return new TollUsage
        {
            City = model.City,
            State = model.State,
            TollDescription = model.TollDescription,
            Value = model.Value,
            VehicleType = model.VehicleType,
        };
    }

    public static CreateTollUsageResponseModel ToServiceModel(this TollUsage domain)
    {
        return new CreateTollUsageResponseModel
        {
            Id = domain.Id,
            CreatedDate = domain.CreatedDate,
            City = domain.City,
            State = domain.State,
            TollDescription = domain.TollDescription,
            Value = domain.Value,
            VehicleType = domain.VehicleType,
        };
    }
}

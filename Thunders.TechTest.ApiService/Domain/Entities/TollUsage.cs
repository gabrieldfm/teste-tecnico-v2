﻿using Thunders.TechTest.ApiService.Domain.Enumerations;

namespace Thunders.TechTest.ApiService.Domain.Entities;

public class TollUsage
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; private set; }
    public string TollDescription { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public decimal Value { get; set; }
    public VehicleType VehicleType { get; set; }

    public void SetUsageDate()
    {
        CreatedDate = DateTime.Now;
    }
}

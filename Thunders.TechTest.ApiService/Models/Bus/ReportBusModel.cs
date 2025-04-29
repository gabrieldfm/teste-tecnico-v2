using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Thunders.TechTest.ApiService.Models.Bus;

[ExcludeFromCodeCoverage]
public class ReportBusModel
{
    public string Id { get; set; } = null!;

    public string ReportType { get; set; } = null!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? NumberOfRecords { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? BaseMonth { get; set; }
}
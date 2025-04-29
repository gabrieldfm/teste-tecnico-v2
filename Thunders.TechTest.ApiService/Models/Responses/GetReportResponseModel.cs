using System.Diagnostics.CodeAnalysis;

namespace Thunders.TechTest.ApiService.Models.Responses;

[ExcludeFromCodeCoverage]
public class GetReportResponseModel
{
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = "text/csv";
    public byte[] FileStream { get; set; } = null!;
}

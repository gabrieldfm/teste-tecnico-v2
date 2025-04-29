using System.Diagnostics.CodeAnalysis;

namespace Thunders.TechTest.ApiService.Models.Responses;

[ExcludeFromCodeCoverage]
public class BaseReponseModel<T>
{
    public bool HasError { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? ResponseData { get; set; }
}

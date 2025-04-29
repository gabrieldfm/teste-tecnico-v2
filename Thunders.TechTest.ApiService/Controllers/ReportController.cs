using Microsoft.AspNetCore.Mvc;
using Thunders.TechTest.ApiService.Services.Report;

namespace Thunders.TechTest.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController(IReportService reportService) : Controller
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
    {
        var response = await reportService.GetReportAsync(id);
        if (response.HasError)
        {
            return NotFound(response.Message);
        }
        else if (!string.Equals(response.Message, "Success", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(response.Message);
        }

        return File(response.ResponseData!.FileStream, response.ResponseData.ContentType, response.ResponseData.FileName);
    }
}

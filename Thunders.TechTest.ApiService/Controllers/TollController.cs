using Microsoft.AspNetCore.Mvc;
using Thunders.TechTest.ApiService.Models.Requests;
using Thunders.TechTest.ApiService.Services.Toll;

namespace Thunders.TechTest.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TollController(ITollService tollService) : Controller
{
    [HttpPost("usage")]
    public async Task<IActionResult> CreateUsageAsync([FromBody] CreateTollUsageRequestModel request)
    {
        var response = await tollService.RegisterUsageAsync(request);
        if (response.HasError) 
        {
            return BadRequest(response.Message);
        }

        return Ok(response.ResponseData);
    }

    [HttpGet("total-by-hour-and-city")]
    public async Task<IActionResult> GetTotalRevenueByHourAndCityAsync()
    {
        var response = await tollService.GetTotalRevenueByHourAndCityAsync();
        if (response.HasError)
        {
            return BadRequest(response.Message);
        }

        return Accepted(response.ResponseData);
    }
    
    [HttpGet("veichle-by-toll")]
    public async Task<IActionResult> GetVeichleByTollAsync()
    {
        var response = await tollService.GetVeichleByTollAsync();
        if (response.HasError)
        {
            return BadRequest(response.Message);
        }

        return Accepted(response.ResponseData);
    }
    
    [HttpGet("top-revenue-by-month")]
    public async Task<IActionResult> GetTopRevenueByMonthAsync([FromQuery] GetTopRevenueByMonthRequestModel request)
    {
        var response = await tollService.GetTopRevenueByMonthAsync(request);
        if (response.HasError)
        {
            return BadRequest(response.Message);
        }

        return Accepted(response.ResponseData);
    }
}

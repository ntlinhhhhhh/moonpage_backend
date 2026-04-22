using DiaryApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DiaryApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/statistics")]
public class StatisticsController(IStatisticsService statisticsService) : ControllerBase 
{
    private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                                 ?? User.FindFirst("id")?.Value 
                                 ?? "";

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int? year, [FromQuery] int? month)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
        {
            return Unauthorized(new { message = "User identification missing from token." });
        }

        // Mặc định lấy năm hiện tại
        int queryYear = year ?? DateTime.UtcNow.Year;

        var result = await statisticsService.GetStatsSummaryAsync(CurrentUserId, queryYear, month);

        if (result == null)
        {
            return NotFound(new { message = "No statistical data found for this period." });
        }

        return Ok(result);
    }
}
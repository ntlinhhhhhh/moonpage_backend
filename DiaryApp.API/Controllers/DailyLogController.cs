using System.Security.Claims;
using DiaryApp.Application.DTOs.DailyLog;
using DiaryApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiaryApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/dailylogs")]
public class DailyLogController(IDailyLogService logService) : ControllerBase
{
    private readonly IDailyLogService _logService = logService;

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // POST: api/dailylogs
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpsertDailyLog([FromForm] DailyLogRequestDto request) // Đổi FromBody thành FromForm
    {
        try
        {
            await _logService.UpsertLogAsync(CurrentUserId, request);
            return Ok(new { message = "Your log was saved successfully!" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/dailylogs/date/2026-03-14
    [HttpGet("date/{date}")]
    public async Task<IActionResult> GetLogByDate(string date)
    {
        try
        {
            var log = await _logService.GetLogByDateAsync(CurrentUserId, date);
            
            if (log == null) return NotFound(new { message = "You don't have a log for this date yet." });
            return Ok(log);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/dailylogs/month/2026-03
    [HttpGet("month/{yearMonth}")]
    public async Task<IActionResult> GetLogsByMonth(string yearMonth)
    {
        try
        {
            var logs = await _logService.GetLogsByMonthAsync(CurrentUserId, yearMonth);
            return Ok(logs);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/dailylogs/activity/act123/month/2026-03
    [HttpGet("activity/{activityId}/month/{yearMonth}")]
    public async Task<IActionResult> GetLogsByActivity(string activityId, string yearMonth)
    {
        try
        {
            var logs = await _logService.GetLogsByActivityAsync(CurrentUserId, activityId, yearMonth);
            return Ok(logs);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/dailylogs/mood/3
    [HttpGet("mood/{moodId}")]
    public async Task<IActionResult> GetLogsByMood(int moodId)
    {
        try
        {
            var logs = await _logService.GetLogsByMoodAsync(CurrentUserId, moodId);
            return Ok(logs);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/dailylogs/menstruation?isMenstruation=true
    [HttpGet("menstruation")]
    public async Task<IActionResult> GetLogsByMenstruation([FromQuery] bool isMenstruation)
    {
        try
        {
            var logs = await _logService.GetLogsByMenstruationAsync(CurrentUserId, isMenstruation);
            return Ok(logs);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/dailylogs/search?keyword=vui
    [HttpGet("search")]
    public async Task<IActionResult> SearchByNote([FromQuery] string keyword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Ok(new List<DailyLogResponseDto>());

            var logs = await _logService.SearchByNoteAsync(CurrentUserId, keyword);
            return Ok(logs);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE: api/dailylogs/date/2026-03-14
    [HttpDelete("date/{date}")]
    public async Task<IActionResult> DeleteLog(string date)
    {
        try
        {
            await _logService.DeleteLogAsync(CurrentUserId, date);
            return Ok(new { message = "Your log has been deleted successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
using System.Diagnostics;
using System.Security.Claims;
using DiaryApp.Application.DTOs.DailyLog;
using DiaryApp.Application.DTOs.Moment;
using DiaryApp.Application.Interfaces;
using DiaryApp.Application.Interfaces.Services;
using FirebaseAdmin.Messaging;
using Google.Protobuf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiaryApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/moments")]
public class MomentController(
    IMomentService momentService,
    IDailyLogService dailyLogService,
    IMessageProducer messageProducer,
    ILogger<MomentController> logger
) : ControllerBase
{
    private readonly IMomentService _momentService = momentService;
    private readonly IDailyLogService _dailyLogService = dailyLogService;
    private readonly IMessageProducer _messageProducer = messageProducer;
    private readonly ILogger<MomentController> _logger = logger;

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // POST: api/moments
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateMoment([FromForm] MomentRequestDto request)
    {
        if (request.ImageFile == null || request.ImageFile.Length == 0)
            return BadRequest(new { message = "Please attach the photo!" });

        try
        {
            var response = await _momentService.CreateMomentAsync(CurrentUserId, request);
            
            return Accepted(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating moment", ex });
        }
    }

    // GET: api/moments/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMomentById(string id)
    {
        try
        {
            var moment = await _momentService.GetByIdAsync(id);
            if (moment == null) return NotFound(new { message = "We couldn't find this moment." });
            
            return Ok(moment);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/moments/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMyMoments()
    {
        try
        {
            var moments = await _momentService.GetMomentsByUserIdAsync(CurrentUserId);
            return Ok(moments);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/moments/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetMomentsByUser(string userId)
    {
        try
        {
            var moments = await _momentService.GetMomentsByUserIdAsync(userId);
            return Ok(moments);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE: api/moments/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMoment(string id)
    {
        try
        {
            await _momentService.DeleteAsync(CurrentUserId, id);
            return Ok(new { message = "Your moment has been deleted successfully." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
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
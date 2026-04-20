using System.Diagnostics;
using System.Security.Claims;
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
    IMessageProducer messageProducer
) : ControllerBase
{
    private readonly IMomentService _momentService = momentService;
    private readonly IMessageProducer _messageProducer = messageProducer;

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // POST: api/moments
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateMoment([FromForm] MomentRequestDto request)
    {
        if (request.ImageFile == null || request.ImageFile.Length == 0)
        {
            return BadRequest(new { message = "Please attach a photo to share your moment!"});
        } 
        try
        {
            var tempFilePath = Path.GetTempFileName();
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await request.ImageFile.CopyToAsync(stream);
            }

            var payload = new
            {
                UserId = CurrentUserId,
                DailyLogId = request.DailyLogId,
                Caption = request.Caption,
                IsPublic = request.IsPublic,
                CapturedAt = request.CapturedAt,
                TempImagePath = tempFilePath
            };

            await _messageProducer.SendMessageAsync(payload, "image_upload_queue");

            return Accepted(new { 
                success = true, 
                message = "Your moment is uploading in the background. It'll be ready soon!" 
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Server error: {ex.Message}" });
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
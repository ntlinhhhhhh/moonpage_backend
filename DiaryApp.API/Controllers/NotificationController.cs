using Microsoft.AspNetCore.Mvc;
using DiaryApp.Application.Interfaces;
using DiaryApp.Application.DTOs.Notification;
using Microsoft.AspNetCore.Authorization;
using DiaryApp.Application.DTOs;
using System.Security.Claims;

namespace DiaryApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationController(
    IFirebaseNotificationService firebaseNotificationService,
    IAppNotificationService appNotificationService) : ControllerBase
{
    private readonly IFirebaseNotificationService _firebaseService = firebaseNotificationService;
    private readonly IAppNotificationService _appNotificationService = appNotificationService;
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;


    [HttpPost("send")]
    public async Task<IActionResult> SendPushNotification([FromBody] PushNotificationRequestDto request)
    {
        try
        {
            var messageId = await _firebaseService.SendPushNotificationAsync(
                request.Token, 
                request.Title, 
                request.Body);
                
            return Ok(new { Success = true, MessageId = messageId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] AppNotificationRequestDto request)
    {
        try
        {
            var result = await _appNotificationService.CreateNotificationAsync(request);
            return StatusCode(201, new { Success = true, Data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized(new { Success = false, Message = "Please log in to continue!" });

        try
        {
            var notifications = await _appNotificationService.GetMyNotificationsAsync(userId);
            return Ok(new { Success = true, Data = notifications });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            await _appNotificationService.MarkAsReadAsync(id, userId);
            return NoContent(); 
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Success = false, Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(string id)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            await _appNotificationService.DeleteNotificationAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Success = false, Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }

    [HttpDelete("all")]
    public async Task<IActionResult> DeleteAllMyNotifications()
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            await _appNotificationService.DeleteAllMyNotificationsAsync(userId);
            return Ok(new { Success = true, Message = "All your notifications have been cleared!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }
}
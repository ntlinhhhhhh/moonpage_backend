using System.Security.Claims;
using DiaryApp.Application.DTOs.User;
using DiaryApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiaryApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UserController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET: api/users
    [Authorize(Roles = "Admin")]
    [HttpGet("")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE: api/users/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return Ok(new { message = "User deleted successfully." });
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

    // GET: api/users/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var profile = await _userService.GetProfileAsync(CurrentUserId);
            return Ok(profile);
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

    // PUT: api/users/me
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequestDto request)
    {
        try
        {
            await _userService.UpdateProfileAsync(CurrentUserId, request);
            return Ok(new { message = "Your profile has been updated successfully!" });
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

    // GET: api/users/search?name=abc&limit=10
    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string name, [FromQuery] int limit)
    {
        try
        {
            var users = await _userService.SearchUsersAsync(name, limit);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/users/me/themes
    [HttpGet("me/themes")]
    public async Task<IActionResult> GetMyThemes()
    {
        try
        {
            var themes = await _userService.GetMyThemeIdsAsync(CurrentUserId);
            return Ok(themes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST: api/users/me/themes/buy
    [HttpPost("me/themes/buy")]
    public async Task<IActionResult> BuyTheme([FromBody] BuyThemeRequestDto request)
    {
        try
        {
            await _userService.BuyThemeAsync(CurrentUserId, request);
            return Ok(new { message = "Theme purchased successfully!" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message }); 
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message }); 
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT: api/users/me/themes/active
    [HttpPut("me/themes/active")]
    public async Task<IActionResult> ChangeTheme([FromBody] UpdateThemeRequestDto request)
    {
        try
        {
            await _userService.ChangeActiveThemeAsync(CurrentUserId, request);
            return Ok(new { message = "Your new theme has been applied successfully!" });
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
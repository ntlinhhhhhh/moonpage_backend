using System.Security.Claims;
using DiaryApp.Application.DTOs.Theme;
using DiaryApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DiaryApp.API.Controllers;

[Authorize]
[Route("api/themes")]
[ApiController]
public class ThemeController(IThemeService themeService) : ControllerBase
{
    private readonly IThemeService _themeService = themeService;

    // GET: /api/themes
    [HttpGet]
    public async Task<IActionResult> GetAllActiveThemes()
    {
        try
        {
            var themes = await _themeService.GetAllActiveThemesAsync();
            return Ok(themes);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Server error: " + ex.Message });
        }
    }

    // GET: /api/themes/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMyThemes()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var themes = await _themeService.GetThemesByAuthorIdAsync(userId);
            return Ok(themes);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Server error: " + ex.Message });
        }
    }

    // GET: /api/themes/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetThemeById(string id)
    {
        try
        {
            var theme = await _themeService.GetThemeByIdAsync(id);
            
            if (theme == null) 
            {
                return NotFound(new { message = "This theme doesn't exist or is no longer available." });
            }

            return Ok(theme);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Server error: " + ex.Message });
        }
    }

    // GET: /api/themes/{id}/moods
    [HttpGet("{id}/moods")]
    public async Task<IActionResult> GetThemeMoods(string id)
    {
        try
        {
            var moods = await _themeService.GetThemeMoodsAsync(id);
            
            if (!moods.Any()) return NotFound(new { message = "We couldn't find any icons for this theme." });

            return Ok(moods);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Server error: " + ex.Message });
        }
    }

    // POST: /api/themes/list
    [HttpPost("list")]
    public async Task<IActionResult> CreateThemesList([FromBody] List<CreateThemeRequestDto> requests)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var isAdmin = User.IsInRole("Admin");

            await _themeService.CreateThemesListAsync(userId, isAdmin, requests);
            
            return Ok(new { message = $"{requests.Count} themes created successfully!!" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Server error: " + ex.Message }); 
        }
    }

    // POST: /api/themes/upload
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadTheme([FromForm] UploadThemeRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var isAdmin = User.IsInRole("Admin");

            await _themeService.UploadThemeAsync(userId, isAdmin, request);
            
            return Ok(new { message = "Theme uploaded and created successfully!" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Server error: " + ex.Message }); 
        }
    }

    // PUT: /api/themes/{id}
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateTheme(string id, [FromForm] UploadThemeRequestDto request)
    {
        if (id != request.Id) return BadRequest(new { message = "Theme ID mismatch." });
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var isAdmin = User.IsInRole("Admin");

            await _themeService.UpdateThemeAsync(userId, isAdmin, request);
            
            return Ok(new { message = "Theme updated successfully!" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Server error: " + ex.Message }); 
        }
    }

    // DELETE: /api/themes/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTheme(string id)
    {
        try
        {
            await _themeService.DeleteThemeAsync(id);
            return Ok(new { message = "Theme deleted successfully!" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Server error: " + ex.Message });
        }
    }
}
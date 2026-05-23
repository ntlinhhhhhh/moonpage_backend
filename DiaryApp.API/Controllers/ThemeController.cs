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

    // POST: /api/themes (Upload)
    [HttpPost]
    public async Task<IActionResult> CreateTheme([FromForm] UploadThemeRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (request.IsOfficial && role != "Admin")
            {
                return Forbid();
            }

            await _themeService.CreateThemeAsync(userId, request);
            
            return Ok(new { message = "Theme created successfully!!" });
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

    // POST: /api/themes/list (Admin Only)
    [Authorize(Roles = "Admin")]
    [HttpPost("list")]
    public async Task<IActionResult> CreateThemesList([FromBody] List<CreateThemeRequestDto> requests)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _themeService.CreateThemesListAsync(userId, requests);
            
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

    // PUT: /api/themes/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTheme(string id, [FromForm] UploadThemeRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            var existingTheme = await _themeService.GetThemeByIdAsync(id);
            if (existingTheme == null) return NotFound(new { message = "Theme not found." });

            // Only author or admin can update
            if (existingTheme.AuthorId != userId && role != "Admin")
            {
                return Forbid();
            }

            if (request.IsOfficial && role != "Admin")
            {
                return Forbid();
            }

            await _themeService.UpdateThemeAsync(id, request);
            return Ok(new { message = "Theme updated successfully!" });
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
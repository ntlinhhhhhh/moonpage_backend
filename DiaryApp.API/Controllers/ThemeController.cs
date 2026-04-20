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

    // POST: /api/themes
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTheme([FromBody] CreateThemeRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _themeService.CreateThemeAsync(request);
            
            return CreatedAtAction(nameof(GetThemeById), new { id = request.Id }, new { message = "Theme created successfully!!" }); // 201 Created
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTheme(string id, [FromBody] CreateThemeRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _themeService.UpdateThemeAsync(id, request);
            return Ok(new { message = "Theme updated successfully!" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Server error: " + ex.Message }); // 500
        }
    }

    // DELETE: /api/themes/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
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
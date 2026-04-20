using DiaryApp.Application.DTOs.Activity;
using DiaryApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiaryApp.API.Controllers;

[Authorize]
[ApiController]
[Route("api/activities")]
public class ActivityController(IActivityService activityService) : ControllerBase
{
    private readonly IActivityService _activityService = activityService;

    // GET: api/activities
    [HttpGet]
    public async Task<IActionResult> GetAllActivities()
    {
        try
        {
            var activities = await _activityService.GetAllActivitiesAsync();
            return Ok(activities);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server error: " + ex.Message });
        }
    }

    // GET: api/activities/category/{category}
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetActivitiesByCategory(string category)
    {
        try
        {
            var activities = await _activityService.GetActivitiesByCategoryAsync(category);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server error: " + ex.Message });
        }
    }

    // GET: api/activities/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetActivityById(string id)
    {
        try
        {
            var activity = await _activityService.GetActivityByIdAsync(id);
            return Ok(activity);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server error: " + ex.Message });
        }
    }

    // POST: api/activities
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateActivity([FromBody] ActivityRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var createdActivity = await _activityService.CreateActivityAsync(request);
            return CreatedAtAction(nameof(GetActivityById), new { id = createdActivity.Id }, createdActivity);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT: api/activities/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateActivity(string id, [FromBody] ActivityRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _activityService.UpdateActivityAsync(id, request);
            return Ok(new { message = "Your activity has been updated successfully!" });
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

    // DELETE: api/activities/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteActivity(string id)
    {
        try
        {
            await _activityService.DeleteActivityAsync(id);
            return Ok(new { message = "Your activity has been deleted successfully!" });
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
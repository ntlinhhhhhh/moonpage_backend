using DiaryApp.Application.DTOs.Activity;

namespace DiaryApp.Application.Interfaces.Services;

public interface IActivityService
{
    Task<ActivityResponseDto> CreateActivityAsync(ActivityRequestDto request);
    Task UpdateActivityAsync(string id, ActivityRequestDto request);
    Task DeleteActivityAsync(string activityId);
    Task<IEnumerable<ActivityResponseDto>> GetAllActivitiesAsync();
    Task<IEnumerable<ActivityResponseDto>> GetActivitiesByCategoryAsync(string category);
    Task<ActivityResponseDto?> GetActivityByIdAsync(string activityId);
}
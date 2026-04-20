using DiaryApp.Application.DTOs.Activity;
using DiaryApp.Application.Interfaces;
using DiaryApp.Application.Interfaces.Services;
using DiaryApp.Domain.Entities;

namespace DiaryApp.Application.Services;

public class ActivityService(
    IActivityRepository activityRepository,
    IRedisCacheService cacheService
    ) : IActivityService
{
    private readonly IActivityRepository _activityRepository = activityRepository;
    private readonly IRedisCacheService _cacheService = cacheService;
    private readonly TimeSpan _cacheTtl = TimeSpan.FromDays(1);

    public async Task<ActivityResponseDto> CreateActivityAsync(ActivityRequestDto request)
    {
        var newActivity = new Activity
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            IconUrl = request.IconUrl,
            Category = request.Category ?? "Other"
        };

        await _activityRepository.CreateAsync(newActivity);
        await ClearActivityCachesAsync(newActivity.Category);
        return MapToDto(newActivity);
    }

    public async Task UpdateActivityAsync(string id, ActivityRequestDto request)
    {
        var existingActivity = await _activityRepository.GetByIdAsync(id);
        if (existingActivity == null)
        {
            throw new KeyNotFoundException("We couldn't find the activity you're trying to update.");
        }

        string oldCategory = existingActivity.Category;

        existingActivity.Name = request.Name;
        existingActivity.IconUrl = request.IconUrl;
        existingActivity.Category = request.Category ?? "Other";

        await _activityRepository.UpdateAsync(existingActivity);

        await _cacheService.RemoveAsync($"activity:{id}");
        await ClearActivityCachesAsync(existingActivity.Category);
        
        if (oldCategory != existingActivity.Category)
        {
            await ClearActivityCachesAsync(oldCategory); 
        }
    }

    public async Task DeleteActivityAsync(string activityId)
    {
        var existingActivity = await _activityRepository.GetByIdAsync(activityId);
        if (existingActivity == null)
        {
            throw new KeyNotFoundException("The activity you're trying to delete doesn't exist.");
        }

        await _activityRepository.DeleteAsync(activityId);
        await _cacheService.RemoveAsync($"activity:{activityId}");
        await ClearActivityCachesAsync(existingActivity.Category);
    }

    public async Task<IEnumerable<ActivityResponseDto>> GetAllActivitiesAsync()
    {
        string cacheKey = "activities:all";
        var cachedActivities = await _cacheService.GetAsync<IEnumerable<ActivityResponseDto>>(cacheKey);
        if (cachedActivities != null) return cachedActivities;
        
        var activities = await _activityRepository.GetAllAsync();
        var dtos = activities.Select(MapToDto).ToList();
        await _cacheService.SetAsync(cacheKey, dtos, _cacheTtl);
        return dtos;
    }

    public async Task<IEnumerable<ActivityResponseDto>> GetActivitiesByCategoryAsync(string category)
    {
        string cacheKey = $"activities:category:{category}";

        var cachedActivities = await _cacheService.GetAsync<IEnumerable<ActivityResponseDto>>(cacheKey);
        if (cachedActivities != null) return cachedActivities;

        var activities = await _activityRepository.GetByCategoryAsync(category);
        var dtos = activities.Select(MapToDto).ToList();

        await _cacheService.SetAsync(cacheKey, dtos, _cacheTtl);

        return dtos;
    }

    public async Task<ActivityResponseDto?> GetActivityByIdAsync(string activityId)
    {
        string cacheKey = $"activity:{activityId}";

        var cachedActivity = await _cacheService.GetAsync<ActivityResponseDto>(cacheKey);
        if (cachedActivity != null) return cachedActivity;

        var activity = await _activityRepository.GetByIdAsync(activityId);
        if (activity == null) return null;

        var dto = MapToDto(activity);
        await _cacheService.SetAsync(cacheKey, dto, _cacheTtl);
        
        return dto;
    }

    private static ActivityResponseDto MapToDto(Domain.Entities.Activity activity)
    {
        return new ActivityResponseDto
        {
            Id = activity.Id,
            Name = activity.Name,
            IconUrl = activity.IconUrl,
            Category = activity.Category ?? "Other"
        };
    }

    private async Task ClearActivityCachesAsync(string category)
    {
        await _cacheService.RemoveAsync("activities:all");
        
        if (!string.IsNullOrEmpty(category))
        {
            await _cacheService.RemoveAsync($"activities:category:{category}");
        }
    }
}
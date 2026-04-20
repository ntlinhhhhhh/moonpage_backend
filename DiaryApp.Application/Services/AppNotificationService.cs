using DiaryApp.Application.DTOs;
using DiaryApp.Application.DTOs.Notification; 
using DiaryApp.Application.Interfaces;
using DiaryApp.Application.Interfaces.Services;
using DiaryApp.Domain.Entities;

namespace DiaryApp.Application.Services;

public class AppNotificationService(
    IAppNotificationRepository notificationRepository,
    IRedisCacheService cacheService
    ) : IAppNotificationService
{
    private readonly IAppNotificationRepository _notificationRepository = notificationRepository;
    private readonly IRedisCacheService _cacheService = cacheService;

    private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(30);

    public async Task<AppNotificationResponseDto> CreateNotificationAsync(AppNotificationRequestDto request)
    {
        var newNotification = new AppNotification
        {
            Id = Guid.NewGuid().ToString(),
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type ?? "System",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.CreateAsync(newNotification);
        await _cacheService.RemoveAsync($"notifications:{request.UserId}");

        return MapToDto(newNotification);
    }

    public async Task<IEnumerable<AppNotificationResponseDto>> GetMyNotificationsAsync(string userId)
    {
        string cacheKey = $"notifications:{userId}";
        var cachedNotifications = await _cacheService.GetAsync<IEnumerable<AppNotificationResponseDto>>(cacheKey);
        if (cachedNotifications != null) return cachedNotifications;

        var notifications = await _notificationRepository.GetByUserIdAsync(userId);
        var dtos = notifications.Select(MapToDto).ToList();

        await _cacheService.SetAsync(cacheKey, dtos, _cacheTtl);

        return dtos;
    }

    public async Task MarkAsReadAsync(string notificationId, string currentUserId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        
        if (notification == null)
        {
            throw new KeyNotFoundException("We couldn't find that notification.");
        }

        if (notification.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You don't have permission to modify this notification.");
        }

        if (!notification.IsRead)
        {
            await _notificationRepository.MarkAsReadAsync(notificationId);
        }
        await _cacheService.RemoveAsync($"notifications:{currentUserId}");
    }

    public async Task DeleteNotificationAsync(string notificationId, string currentUserId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        
        if (notification == null)
        {
            throw new KeyNotFoundException("The notification you're trying to delete doesn't exist.");
        }

        if (notification.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this notification.");
        }

        await _notificationRepository.DeleteByIdAsync(notificationId);
        await _cacheService.RemoveAsync($"notifications:{currentUserId}");
    }

    public async Task DeleteAllMyNotificationsAsync(string userId)
    {
        await _notificationRepository.DeleteAllByUserIdAsync(userId);
        await _cacheService.RemoveAsync($"notifications:{userId}");
    }

    private static AppNotificationResponseDto MapToDto(AppNotification notification)
    {
        return new AppNotificationResponseDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }
}
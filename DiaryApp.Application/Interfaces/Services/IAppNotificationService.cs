using DiaryApp.Application.DTOs;
using DiaryApp.Application.DTOs.Notification;

namespace DiaryApp.Application.Interfaces;

public interface IAppNotificationService
{
    Task<AppNotificationResponseDto> CreateNotificationAsync(AppNotificationRequestDto request);
    Task<IEnumerable<AppNotificationResponseDto>> GetMyNotificationsAsync(string userId);
    Task MarkAsReadAsync(string notificationId, string currentUserId);
    Task DeleteNotificationAsync(string notificationId, string currentUserId);
    Task DeleteAllMyNotificationsAsync(string userId);
}
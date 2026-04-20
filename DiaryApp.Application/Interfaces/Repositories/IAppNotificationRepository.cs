using DiaryApp.Domain.Entities;

namespace DiaryApp.Application.Interfaces;

public interface IAppNotificationRepository
{
    Task CreateAsync(AppNotification notification);
    Task <AppNotification> GetByIdAsync(string id);
    Task<IEnumerable<AppNotification>> GetByUserIdAsync(string userId);
    Task MarkAsReadAsync(string notificationId);
    Task DeleteByIdAsync(string notificationId);
    Task DeleteAllByUserIdAsync(string userId);
}
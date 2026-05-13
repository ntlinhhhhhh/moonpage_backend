using DiaryApp.Application.Interfaces;
using DiaryApp.Domain.Entities;
using DiaryApp.Infrastructure.Data;
using Google.Cloud.Firestore;

namespace DiaryApp.Infrastructure.Repositories;

public class AppNotificationRepository : IAppNotificationRepository
{
    private readonly FirestoreDb _db;
    private readonly CollectionReference _notificationCollection;

    public AppNotificationRepository(FirestoreProvider provider)
    {
        _db = provider.Database;
        _notificationCollection = _db.Collection("notifications");
    }

    async Task IAppNotificationRepository.CreateAsync(AppNotification notification)
    {
        DocumentReference docRef = _notificationCollection.Document(notification.Id);
        var notificationData = MapNotificationToDictionary(notification);
        await docRef.SetAsync(notificationData);
    }

    async Task<IEnumerable<AppNotification>> IAppNotificationRepository.GetByUserIdAsync(string userId)
    {
        Query query = _notificationCollection
                       .WhereEqualTo("UserId", userId)
                       .OrderByDescending("CreatedAt");

        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(MapSnapshotToAppNotification);
    }

    async Task<AppNotification> IAppNotificationRepository.GetByIdAsync(string id)
    {
        DocumentReference docRef = _notificationCollection.Document(id);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return null;
        return MapSnapshotToAppNotification(snapshot);
    }


    async Task IAppNotificationRepository.MarkAsReadAsync(string notificationId)
    {
        DocumentReference docRef = _notificationCollection.Document(notificationId);
        await docRef.UpdateAsync("IsRead", true);
    }

    async Task IAppNotificationRepository.DeleteByIdAsync(string notificationId)
    {
        await _notificationCollection.Document(notificationId).DeleteAsync();
    }

    async Task IAppNotificationRepository.DeleteAllByUserIdAsync(string userId)
    {
        Query query = _notificationCollection.WhereEqualTo("UserId", userId);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        if (snapshot.Documents.Count == 0) return;

        var batch = _db.StartBatch();

        foreach (var doc in snapshot.Documents)
        {
            batch.Delete(doc.Reference);
        }

        await batch.CommitAsync();
    }

    private Dictionary<string, object> MapNotificationToDictionary(AppNotification notification)
    {
        return new Dictionary<string, object>
        {
            { "Id", notification.Id },
            { "UserId",  notification.UserId},
            { "Title", notification.Title },
            { "Message", notification.Message },
            { "Type", notification.Type },
            { "IsRead", notification.IsRead },
            { "CreatedAt", Timestamp.FromDateTime(notification.CreatedAt.ToUniversalTime()) }
        };
    }

    private AppNotification MapSnapshotToAppNotification(DocumentSnapshot snapshot)
    {
        if (!snapshot.Exists) return null;

        return new AppNotification
        {
            Id = snapshot.Id,
            UserId = snapshot.ContainsField("UserId") ? snapshot.GetValue<string>("UserId") : string.Empty,
            Title = snapshot.ContainsField("Title") ? snapshot.GetValue<string>("Title") : string.Empty,
            Message = snapshot.ContainsField("Message") ? snapshot.GetValue<string>("Message") : string.Empty,
            Type = snapshot.ContainsField("Type") ? snapshot.GetValue<string>("Type") : "System",
            IsRead = snapshot.ContainsField("IsRead") ? snapshot.GetValue<bool>("IsRead") : false,
            CreatedAt = snapshot.ContainsField("CreatedAt") ? snapshot.GetValue<Timestamp>("CreatedAt").ToDateTime() : DateTime.UtcNow
        };
    }
}
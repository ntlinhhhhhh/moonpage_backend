using DiaryApp.Application.Interfaces;
using DiaryApp.Domain.Entities;
using DiaryApp.Infrastructure.Data;
using FirebaseAdmin.Messaging;
using Google.Cloud.Firestore;
using Org.BouncyCastle.Pqc.Crypto.Saber;


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
        var notificationData = MapMomentToDictionary(notification);
        await docRef.SetAsync(notificationData);
    }

    async Task<IEnumerable<AppNotification>> IAppNotificationRepository.GetByUserIdAsync(string userId)
    {
        Query query = _db.Collection("Notifications")
                       .WhereEqualTo("userId", userId)
                       .OrderByDescending("createdAt");

        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        return  snapshot.Documents.Select(MapSnapshotToAppNotification);
    }

    async Task<AppNotification> IAppNotificationRepository.GetByIdAsync(string id)
    {
        DocumentReference docRef = _notificationCollection.Document(id);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        return MapSnapshotToAppNotification(snapshot);
    }


    async Task IAppNotificationRepository.MarkAsReadAsync(string notificationId)
    {
        DocumentReference docRef = _notificationCollection.Document(notificationId);
        await docRef.UpdateAsync("isRead", true);
    }

    async Task IAppNotificationRepository.DeleteByIdAsync(string notificationId)
    {
        await _notificationCollection.Document(notificationId).DeleteAsync();
    }

    async Task IAppNotificationRepository.DeleteAllByUserIdAsync(string userId)
    {
        Query query = _notificationCollection.WhereEqualTo("userId", userId);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        if (snapshot.Documents.Count == 0) return;

        var batch = _db.StartBatch();

        foreach (var doc in snapshot.Documents)
        {
            batch.Delete(doc.Reference);
        }

        await batch.CommitAsync(); 
    }

        private Dictionary<string, object> MapMomentToDictionary(AppNotification notification)
    {
        return new Dictionary<string, object>
        {
            { "id", notification.Id },
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
        return new AppNotification
        {
            Id = snapshot.GetValue<string>("id"),
            UserId = snapshot.GetValue<string>("userId"),
            Title = snapshot.GetValue<string>("title"),
            Message = snapshot.GetValue<string>("message"),
            Type = snapshot.GetValue<string>("type"),
            IsRead = snapshot.GetValue<bool>("isRead"),
            CreatedAt = snapshot.GetValue<Timestamp>("createdAt").ToDateTime()
        };
    }
}
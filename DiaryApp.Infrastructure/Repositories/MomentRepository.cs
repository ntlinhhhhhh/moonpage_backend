using DiaryApp.Application.Interfaces;
using DiaryApp.Domain.Entities;
using DiaryApp.Infrastructure.Data;
using Google.Cloud.Firestore;

namespace DiaryApp.Infrastructure.Repositories;

public class MomentRepository : IMomentRepository
{
    private readonly FirestoreDb _db;
    private readonly CollectionReference _momentCollection;

    public MomentRepository(FirestoreProvider provider)
    {
        _db = provider.Database;
        _momentCollection = _db.Collection("moments");
    }

    async Task IMomentRepository.CreateAsync(Moment moment)
    {
        DocumentReference docRef = _momentCollection.Document(moment.Id);

        var momentData = MapMomentToDictionary(moment);
        await docRef.SetAsync(momentData);
    }

   async Task IMomentRepository.UpdateAsync(Moment moment)
    {
        if (string.IsNullOrEmpty(moment.Id))
        {
            throw new ArgumentException("Moment ID cannot be empty when updating.");
        }

        DocumentReference docRef = _momentCollection.Document(moment.Id);

        var momentData = MapMomentToDictionary(moment);

        await docRef.SetAsync(momentData, SetOptions.MergeAll);
    }

    async Task IMomentRepository.DeleteAsync(string momentId)
    {
        await _momentCollection.Document(momentId).DeleteAsync();
    }

    async Task<Moment?> IMomentRepository.GetByIdAsync(string id)
    {
        DocumentReference docRef = _momentCollection.Document(id);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists) return null;

        return MapSnapshotToMoment(snapshot);
    }

    async Task<IEnumerable<Moment>> IMomentRepository.GetMomentsByUserIdAsync(string userId)
    {
        Query query = _momentCollection.WhereEqualTo("UserId", userId);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        return snapshot.Documents.Select(MapSnapshotToMoment);
    }

async Task IMomentRepository.SyncUserMediaInMomentsAsync(string userId, string newName, string newAvatarUrl)
    {
        Query query = _momentCollection.WhereEqualTo("UserId", userId);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        var documents = snapshot.Documents.ToList();
        if (!documents.Any()) return;

        const int batchSize = 400; 

        for (int i = 0; i < documents.Count; i += batchSize)
        {
            var chunk = documents.Skip(i).Take(batchSize);
            WriteBatch batch = _db.StartBatch();

            foreach (DocumentSnapshot document in chunk)
            {
                var updates = new Dictionary<string, object>
                {
                    { "UserName", newName },
                    { "UserAvatarUrl", newAvatarUrl }
                };
                batch.Update(document.Reference, updates);
            }

            await batch.CommitAsync();
        }
    }

    private Dictionary<string, object> MapMomentToDictionary(Moment moment)
    {
        return new Dictionary<string, object>
        {
            { "UserId",  moment.UserId},
            { "UserName", moment.UserName },
            { "UserAvatarUrl", moment.UserAvatarUrl },
            { "DailyLogId", moment.DailyLogId },
            { "ImageUrl", moment.ImageUrl },
            { "Caption", moment.Caption ?? "" },
            { "IsPublic", moment.IsPublic },
            { "CapturedAt", Timestamp.FromDateTime(moment.CapturedAt.ToUniversalTime()) }
        };
    }

    private Moment MapSnapshotToMoment(DocumentSnapshot snapshot)
    {
        return new Moment
        {
            Id = snapshot.Id,
            UserId = snapshot.GetValue<string>("UserId"),
            UserName = snapshot.GetValue<string>("UserName"),
            UserAvatarUrl = snapshot.GetValue<string>("UserAvatarUrl"),
            DailyLogId = snapshot.GetValue<string>("DailyLogId"),
            ImageUrl = snapshot.GetValue<string>("ImageUrl"),
            Caption = snapshot.ContainsField("Caption") ? snapshot.GetValue<string>("Caption") : null,
            IsPublic = snapshot.GetValue<bool>("IsPublic"),
            CapturedAt = snapshot.GetValue<DateTime>("CapturedAt"),
        };
    }
}
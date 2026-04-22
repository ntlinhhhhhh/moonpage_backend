using DiaryApp.Application.Interfaces;
using DiaryApp.Domain.Entities;
using DiaryApp.Infrastructure.Data;
using Google.Cloud.Firestore;

namespace DiaryApp.Infrastructure.Repositories;

public class UserStreakRepository : IUserStreakRepository
{
    private readonly FirestoreDb _db;
    private readonly CollectionReference _streakCollection;

    public UserStreakRepository(FirestoreProvider provider)
    {
        _db = provider.Database;
        _streakCollection = _db.Collection("userStreaks"); 
    }

    async Task<UserStreak?> IUserStreakRepository.GetByUserIdAsync(string userId)
    {
        DocumentReference docRef = _streakCollection.Document(userId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists) return null;

        return MapSnapshotToUserStreak(snapshot);
    }

    async Task IUserStreakRepository.UpsertAsync(UserStreak streak)
    {
        DocumentReference docRef = _streakCollection.Document(streak.UserId);

        var streakData = MapStreakToDictionary(streak);
        
        await docRef.SetAsync(streakData, SetOptions.MergeAll);
    }

    private Dictionary<string, object> MapStreakToDictionary(UserStreak streak)
    {
        var dict = new Dictionary<string, object>
        {
            { "UserId", streak.UserId },
            { "CurrentStreak", streak.CurrentStreak },
            { "LongestStreak", streak.LongestStreak },
            { "StreakFreezes", streak.StreakFreezes },
            { "UpdatedAt", Timestamp.FromDateTime(streak.UpdatedAt.ToUniversalTime()) }
        };

        if (streak.LastLogDate.HasValue)
        {
            dict.Add("LastLogDate", Timestamp.FromDateTime(streak.LastLogDate.Value.ToUniversalTime()));
        }

        return dict;
    }

    private UserStreak MapSnapshotToUserStreak(DocumentSnapshot snapshot)
    {
        return new UserStreak
        {
            UserId = snapshot.Id,
            CurrentStreak = snapshot.ContainsField("CurrentStreak") ? snapshot.GetValue<int>("CurrentStreak") : 0,
            LongestStreak = snapshot.ContainsField("LongestStreak") ? snapshot.GetValue<int>("LongestStreak") : 0,
            StreakFreezes = snapshot.ContainsField("StreakFreezes") ? snapshot.GetValue<int>("StreakFreezes") : 0,
            LastLogDate = snapshot.ContainsField("LastLogDate") ? snapshot.GetValue<DateTime>("LastLogDate") : null,
            UpdatedAt = snapshot.ContainsField("UpdatedAt") ? snapshot.GetValue<DateTime>("UpdatedAt") : DateTime.UtcNow
        };
    }
}
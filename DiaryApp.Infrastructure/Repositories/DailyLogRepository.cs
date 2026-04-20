using DiaryApp.Application.Interfaces;
using DiaryApp.Domain.Entities;
using DiaryApp.Infrastructure.Data;
using Google.Cloud.Firestore;

namespace DiaryApp.Infrastructure.Repositories;

public class DailyLogRepository : IDailyLogRepository
{
    private readonly FirestoreDb _db;
    private readonly CollectionReference _logCollection;

    public DailyLogRepository(FirestoreProvider provider)
    {
        _db = provider.Database;
        _logCollection = _db.Collection("dailyLogs");
    }

    // set template id 
    private string GetDocId(string userId, string date) => $"{userId}_{date}";

    async Task IDailyLogRepository.UpsertAsync(string userId, DailyLog log)
    {
        log.Id = GetDocId(userId, log.Date);
        DocumentReference docRef = _logCollection.Document(log.Id);

        var logData = MapLogToDictionary(userId, log);
        
        // update
        await docRef.SetAsync(logData, SetOptions.MergeAll);
    }

    async Task<DailyLog?> IDailyLogRepository.GetByDateAsync(string userId, string date)
    {
        DocumentReference docRef = _logCollection.Document(GetDocId(userId, date));
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists) return null;

        return MapSnapshotToLog(snapshot);
    }

    async Task<IEnumerable<DailyLog>> IDailyLogRepository.GetLogsByMonthAsync(string userId, string yearMonth)
    {
        Query query = _logCollection
            .WhereEqualTo("UserId", userId)
            .WhereEqualTo("YearMonth", yearMonth)
            .OrderBy("Date");

        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(MapSnapshotToLog);
    }

    // get log by activity
    async Task<IEnumerable<DailyLog>> IDailyLogRepository.GetLogsByActivityAsync(string userId, string activityId, string yearMonth)
    {
        Query query = _logCollection
            .WhereEqualTo("UserId", userId)
            .WhereEqualTo("YearMonth", yearMonth);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        var logs = snapshot.Documents.Select(MapSnapshotToLog);
        return logs.Where(l => l.ActivityIds != null && l.ActivityIds.Contains(activityId));
    }

    // get logs by mood
    async Task<IEnumerable<DailyLog>> IDailyLogRepository.GetLogsByMoodAsync(string userId, int moodId)
    {
        Query query = _logCollection
            .WhereEqualTo("UserId", userId)
            .WhereEqualTo("BaseMoodId", moodId);

        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(MapSnapshotToLog);
    }

    // get logs by menstruation
    async Task<IEnumerable<DailyLog>> IDailyLogRepository.GetLogsByMenstruationAsync(string userId, bool isMenstruation)
    {
        Query query = _logCollection
            .WhereEqualTo("UserId", userId)
            .WhereEqualTo("IsMenstruation", isMenstruation);

        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(MapSnapshotToLog);
    }

    async Task IDailyLogRepository.DeleteAsync(string userId, string dateId)
    {
        await _logCollection.Document(GetDocId(userId, dateId)).DeleteAsync();
    }

    async Task<IEnumerable<DailyLog>> IDailyLogRepository.SearchByNoteAsync(string userId, string keyword)
    {
        Query query = _logCollection
            .WhereEqualTo("UserId", userId)
            .WhereGreaterThanOrEqualTo("Note", keyword)
            .WhereLessThanOrEqualTo("Note", keyword + "\uf8ff");

        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(MapSnapshotToLog);
    }

    private Dictionary<string, object> MapLogToDictionary(string userId, DailyLog log)
    {
        return new Dictionary<string, object>
        {
            { "UserId", userId },
            { "BaseMoodId", log.BaseMoodId },
            { "Date", log.Date },
            { "YearMonth", log.YearMonth },
            { "Note", log.Note ?? "" },
            { "SleepHours", log.SleepHours },
            { "IsMenstruation", log.IsMenstruation },
            { "MenstruationPhase", log.MenstruationPhase ?? "" },
            { "DailyPhotos", log.DailyPhotos },
            { "ActivityIds", log.ActivityIds },
            { "CreatedAt", Timestamp.FromDateTime(log.CreatedAt.ToUniversalTime()) },
            { "UpdatedAt", Timestamp.FromDateTime(DateTime.UtcNow) }
        };
    }

    private DailyLog MapSnapshotToLog(DocumentSnapshot snapshot)
    {
        var log = new DailyLog
        {
            Id = snapshot.Id,
            BaseMoodId = snapshot.GetValue<int>("BaseMoodId"),
            Date = snapshot.GetValue<string>("Date"),
            YearMonth = snapshot.GetValue<string>("YearMonth"),
            Note = snapshot.ContainsField("Note") ? snapshot.GetValue<string>("Note") : null,
            IsMenstruation = snapshot.GetValue<bool>("IsMenstruation"),
            MenstruationPhase = snapshot.ContainsField("MenstruationPhase") ? snapshot.GetValue<string>("MenstruationPhase") : null,
            SleepHours = snapshot.GetValue<double>("SleepHours"),
            CreatedAt = snapshot.GetValue<DateTime>("CreatedAt"),
            UpdatedAt = snapshot.GetValue<DateTime>("UpdatedAt"),
            DailyPhotos = snapshot.ContainsField("DailyPhotos") 
                ? snapshot.GetValue<List<string>>("DailyPhotos") 
                : new List<string>(),
            ActivityIds = snapshot.ContainsField("ActivityIds")
                ? snapshot.GetValue<List<string>>("ActivityIds")
                : new List<string>()
        };
        return log;
    }
}
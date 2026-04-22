using DiaryApp.Application.Interfaces.Repositories;
using DiaryApp.Domain.Entities;
using DiaryApp.Infrastructure.Data;
using Google.Cloud.Firestore;

namespace DiaryApp.Infrastructure.Repositories;

public class StatisticsRepository : IStatisticsRepository
{
    private readonly CollectionReference _dailyLogsCollection;
    private readonly CollectionReference _momentsCollection;

    public StatisticsRepository(FirestoreProvider provider)
    {
        var db = provider.Database;
        _dailyLogsCollection = db.Collection("dailyLogs");
        _momentsCollection = db.Collection("moments");
    }

    public async Task<int> GetTotalPhotosCountAsync(string userId)
    {
        // 1. Đếm ảnh trong Daily Logs
        var logsSnapshot = await _dailyLogsCollection.WhereEqualTo("UserId", userId).GetSnapshotAsync();
        int dailyPhotos = logsSnapshot.Documents.Sum(doc => 
            doc.ContainsField("DailyPhotos") ? doc.GetValue<List<string>>("DailyPhotos")?.Count ?? 0 : 0);

        // 2. Đếm ảnh trong Moments (Sử dụng hàm Count() để tối ưu)
        var momentCountSnapshot = await _momentsCollection.WhereEqualTo("UserId", userId).Count().GetSnapshotAsync();
        
        return dailyPhotos + (int)momentCountSnapshot.Count;
    }

    public async Task<List<DailyLog>> GetLogsInRangeAsync(string userId, int year, int? month)
    {
        Query query = _dailyLogsCollection.WhereEqualTo("UserId", userId);

        if (month.HasValue)
        {
            // Truy vấn theo YearMonth (yyyy-MM) - Cực nhanh
            string yearMonth = $"{year}-{month.Value:D2}";
            query = query.WhereEqualTo("YearMonth", yearMonth);
        }
        else
        {
            // Truy vấn theo dải ngày của cả năm
            query = query.WhereGreaterThanOrEqualTo("Date", $"{year}-01-01")
                         .WhereLessThanOrEqualTo("Date", $"{year}-12-31");
        }

        var snapshot = await query.OrderBy("Date").GetSnapshotAsync();
        var logs = new List<DailyLog>();

        foreach (var doc in snapshot.Documents)
        {
            var data = doc.ToDictionary();
            var log = new DailyLog
            {
                Id = doc.Id,
                UserId = data.GetValueOrDefault("UserId")?.ToString() ?? userId,
                Date = data.GetValueOrDefault("Date")?.ToString() ?? "",
                YearMonth = data.GetValueOrDefault("YearMonth")?.ToString() ?? ""
            };

            // Ép kiểu an toàn để tránh lỗi int64 vs int32
            if (data.TryGetValue("BaseMoodId", out var moodObj) && moodObj != null)
                log.BaseMoodId = Convert.ToInt32(moodObj);

            if (data.TryGetValue("ActivityIds", out var actObj) && actObj is List<object> actList)
                log.ActivityIds = actList.Select(x => x?.ToString() ?? "").ToList();

            logs.Add(log);
        }

        return logs;
    }
}
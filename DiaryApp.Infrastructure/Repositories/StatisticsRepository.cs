// using Google.Cloud.Firestore;

// public class StatisticsRepository(FirestoreDb db) : IStatisticsRepository {
//     private readonly CollectionReference _usersCollection = db.Collection("users");

//     public async Task<int> GetTotalPhotosCountAsync(string userId) {
//         // Đếm ảnh trong DailyLogs
//         var logsQuery = _usersCollection.Document(userId).Collection("dailyLogs");
//         var snapshot = await logsQuery.GetSnapshotAsync();
//         int dailyPhotos = snapshot.Documents.Sum(d => d.GetValue<List<string>>("DailyPhotos")?.Count ?? 0);

//         // Đếm ảnh trong Moments (Giả sử collection Moments nằm riêng)
//         var momentsQuery = db.Collection("Moments").WhereEqualTo("UserId", userId);
//         var momentCount = await momentsQuery.Count().GetSnapshotAsync();
        
//         return dailyPhotos + (int)momentCount.Count;
//     }

//     public async Task<List<DateTime>> GetAllLogDatesAsync(string userId) {
//         var snapshot = await _usersCollection.Document(userId).Collection("DailyLogs")
//             .Select("Date").GetSnapshotAsync();
//         return snapshot.Documents
//             .Select(d => DateTime.Parse(d.GetValue<string>("Date")))
//             .OrderByDescending(d => d).ToList();
//     }
// }
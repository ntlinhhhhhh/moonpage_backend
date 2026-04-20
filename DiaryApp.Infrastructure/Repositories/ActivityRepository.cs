using DiaryApp.Application.Interfaces;
using DiaryApp.Domain.Entities;
using DiaryApp.Infrastructure.Data;
using Google.Cloud.Firestore;

namespace DiaryApp.Infrastructure.Repositories;

public class ActivityRepository : IActivityRepository
{
    private readonly FirestoreDb _db;
    private readonly CollectionReference _activitiesCollection;

    public ActivityRepository(FirestoreProvider provider)
    {
        _db = provider.Database;
        _activitiesCollection = _db.Collection("activities");
    }

    async Task IActivityRepository.CreateAsync(Activity activity)
    {
        DocumentReference docRef = _activitiesCollection.Document(activity.Id);
        var activityData = MapThemeToDictionary(activity);
        await docRef.SetAsync(activityData);
    }

    async Task IActivityRepository.DeleteAsync(string activityId)
    {
        await _activitiesCollection.Document(activityId).DeleteAsync();
    }

    async Task IActivityRepository.UpdateAsync(Activity activity)
    {
        DocumentReference docRef = _activitiesCollection.Document(activity.Id);
        var activityData = MapThemeToDictionary(activity);
        await docRef.SetAsync(activityData, SetOptions.MergeAll);

    }

    async Task<IEnumerable<Activity>> IActivityRepository.GetAllAsync()
    {
        Query query = _activitiesCollection.OrderBy("Name");
        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(MapSnapshotToActivity);
    }

    async Task<IEnumerable<Activity>> IActivityRepository.GetByCategoryAsync(string category)
    {
        Query query = _activitiesCollection
        .WhereEqualTo("Category", category)
        .OrderBy("Name");

        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(MapSnapshotToActivity);
    }

    async Task<Activity?> IActivityRepository.GetByIdAsync(string id)
    {
        DocumentReference docRef = _activitiesCollection.Document(id);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists) return null;

        return MapSnapshotToActivity(snapshot);
    }

    public async Task<bool> CheckAllActivitiesExistAsync(List<string> activityIds)
    {
        if (activityIds == null || !activityIds.Any()) return true;

        var refs = activityIds.Select(id => _activitiesCollection.Document(id.Trim())).ToList();
        var snapshots = await _db.GetAllSnapshotsAsync(refs);

        return snapshots.All(s => s.Exists);
    }

    private Activity MapSnapshotToActivity(DocumentSnapshot snapshot)
    {
        return new Activity
        {
            Id = snapshot.Id,
            Name = snapshot.GetValue<string>("Name") ?? "Activities name",
            IconUrl = snapshot.GetValue<string>("IconUrl") ?? "",
            Category = snapshot.ContainsField("Category") ? snapshot.GetValue<string>("Category") : "Other"
        };
    }

    private Dictionary<string, object> MapThemeToDictionary(Activity activity)
    {
        return new Dictionary<string, object>
        {
            { "Name", activity.Name ?? "" },
            { "IconUrl", activity.IconUrl },
            { "Category", activity.Category ?? "other"}
        };
    }
}
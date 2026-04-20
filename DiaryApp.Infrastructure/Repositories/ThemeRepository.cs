using DiaryApp.Application.Interfaces;
using DiaryApp.Domain.Entities;
using DiaryApp.Domain.Enums;
using DiaryApp.Infrastructure.Data;
using Google.Cloud.Firestore;

namespace DiaryApp.Infrastructure.Repositories;

public class ThemeRepository : IThemeRepository
{
    private readonly FirestoreDb _db;
    private readonly CollectionReference _themeCollection;

    public ThemeRepository(FirestoreProvider provider)
    {
        _db = provider.Database;
        _themeCollection = _db.Collection("themes");
    }

    async Task IThemeRepository.CreateThemeAsync(Theme theme)
    {
        DocumentReference docRef = _themeCollection.Document(theme.Id);
        
        var themeData = MapThemeToDictionary(theme);
        await docRef.SetAsync(themeData);
    }

    async Task<IEnumerable<Theme>> IThemeRepository.GetAllActiveThemesAsync()
    {
        Query query = _themeCollection.WhereEqualTo("IsActive", true);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        return snapshot.Documents.Select(MapSnapshotToTheme);
    }

    async Task<Theme?> IThemeRepository.GetByIdAsync(string themeId)
    {
        DocumentReference docRef = _themeCollection.Document(themeId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists) return null;

        return MapSnapshotToTheme(snapshot);
    }

    async Task<ThemeMoodIcon?> IThemeRepository.GetMoodIconAsync(string themeId, BaseMood baseMoodId)
    {
        var theme = await ((IThemeRepository)this).GetByIdAsync(themeId);
        if (theme == null || theme.Moods == null) return null;

        return theme.Moods.FirstOrDefault(m => m.BaseMoodId == baseMoodId);
    }

    async Task IThemeRepository.UpdateThemeAsync(Theme theme)
    {
        DocumentReference docRef = _themeCollection.Document(theme.Id);
        var themeData = MapThemeToDictionary(theme);
        
        await docRef.SetAsync(themeData, SetOptions.MergeAll);
    }

    async Task IThemeRepository.DeleteThemeAsync(string themeId)
    {
        await _themeCollection.Document(themeId).DeleteAsync();
    }

    private Dictionary<string, object> MapThemeToDictionary(Theme theme)
    {
        return new Dictionary<string, object>
        {
            { "Name", theme.Name },
            { "Price", theme.Price },
            { "ThumbnailUrl", theme.ThumbnailUrl },
            { "BackgroundUrl", theme.BackgroundUrl },
            { "IsActive", theme.IsActive },
            { "Moods", theme.Moods.Select(m => new Dictionary<string, object>
                {
                    { "BaseMoodId", m.BaseMoodId },
                    { "IconUrl", m.IconUrl },
                    { "CustomName", m.CustomName ?? "" }
                }).ToList() 
            }
        };
    }

    private Theme MapSnapshotToTheme(DocumentSnapshot snapshot)
    {
        var theme = new Theme
        {
            Id = snapshot.Id,
            Name = snapshot.GetValue<string>("Name"),
            Price = snapshot.GetValue<int>("Price"),
            ThumbnailUrl = snapshot.GetValue<string>("ThumbnailUrl"),
            BackgroundUrl = snapshot.GetValue<string>("BackgroundUrl"),
            IsActive = snapshot.GetValue<bool>("IsActive"),
            Moods = new List<ThemeMoodIcon>()
        };

        if (snapshot.ContainsField("Moods"))
        {
            var moodsData = snapshot.GetValue<List<object>>("Moods");
            foreach (var item in moodsData)
            {
                var dict = (Dictionary<string, object>)item;
                int baseMoodId = 0;
                if (dict.TryGetValue("BaseMoodId", out var moodIdObj))
                {
                    baseMoodId = Convert.ToInt32(moodIdObj);
                }

                theme.Moods.Add(new ThemeMoodIcon
                {
                    BaseMoodId = (BaseMood)baseMoodId,
                    IconUrl = dict.TryGetValue("IconUrl", out var iconObj) ? iconObj.ToString() ?? "" : "",
                    CustomName = dict.TryGetValue("CustomName", out var nameObj) ? nameObj.ToString() ?? "" : ""
                });
            }
        }

        return theme;
    }
}
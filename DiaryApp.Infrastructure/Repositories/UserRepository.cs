using DiaryApp.Application.Interfaces;
using DiaryApp.Domain.Entities;
using DiaryApp.Infrastructure.Data;
using Google.Cloud.Firestore;
using BCrypt.Net;
using DiaryApp.Domain.Enums;

namespace DiaryApp.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly FirestoreDb _db;
    private readonly CollectionReference _usersCollection;
    private const string DEFAULT_THEME_ID = "default_theme_id";

    public UserRepository(FirestoreProvider provider)
    {
        _db = provider.Database;
        _usersCollection = _db.Collection("users");
    }

    async Task IUserRepository.AddOwnedThemeAsync(string userId, string themeId)
    {
        DocumentReference docRef = _usersCollection.Document(userId);
        await docRef.UpdateAsync("OwnedThemeIds", FieldValue.ArrayUnion(themeId));
    }

    async Task IUserRepository.CreateAsync(User user)
    {
        DocumentReference docRef = _usersCollection.Document(user.Id);
        var userData = new Dictionary<string, object>
        {
            { "Email", user.Email },
            { "Name", user.Name ?? "" },
            { "NameLower", (user.Name  ?? "").ToLower() },
            { "Role", user.Role.ToString() },
            { "HashPassword", user.HashPassword },
            { "AvatarUrl", user.AvatarUrl ?? "" },
            { "Gender", user.Gender ?? "" },
            { "Birthday", user.Birthday ?? "" },
            { "CoinBalance", user.CoinBalance },
            { "OwnedThemeIds", new List<string> { DEFAULT_THEME_ID } },
            { "ActiveThemeId", DEFAULT_THEME_ID },
            { "AuthProvider", user.AuthProvider },
            { "CreatedAt", Timestamp.FromDateTime(user.CreatedAt.ToUniversalTime()) },
        };

        if (user.UpdatedAt.HasValue)
        {
            userData.Add("UpdatedAt", Timestamp.FromDateTime(user.UpdatedAt.Value.ToUniversalTime()));
        }
        
        await docRef.SetAsync(userData);
    }

    async Task IUserRepository.DeleteAsync(string userId)
    {
        await _usersCollection.Document(userId).DeleteAsync();
    }

    async Task<bool> IUserRepository.ExistsByEmailAsync(string email)
    {
        Query query = _usersCollection.WhereEqualTo("Email", email).Limit(1);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        
        return snapshot.Documents.Count > 0;
    }

    async Task<IEnumerable<User>> IUserRepository.GetAllUsersAsync()
    {
        QuerySnapshot snapshot = await _usersCollection.GetSnapshotAsync();

        var users = snapshot.Documents.Select(MapSnapshotToUser);

        return users;
    }

    async Task<User?> IUserRepository.GetByEmailAsync(string email)
    {
        Query query = _usersCollection.WhereEqualTo("Email", email).Limit(1);
        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        if (snapshot.Documents.Count == 0) return null;

        return MapSnapshotToUser(snapshot.Documents[0]);
    }

    async Task<User?> IUserRepository.GetByIdAsync(string id)
    {
        DocumentReference docRef = _usersCollection.Document(id);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists) return null;

        return MapSnapshotToUser(snapshot);
    }

    async Task<List<string>> IUserRepository.GetOwnedThemeIdsAsync(string userId)
    {
        DocumentReference docRef = _usersCollection.Document(userId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (snapshot.Exists && snapshot.ContainsField("OwnedThemeIds"))
        {
            return snapshot.GetValue<List<string>>("OwnedThemeIds");
        }

        return new List<string>();
    }

    async Task<IEnumerable<User>> IUserRepository.SearchByNameAsync(string name, int limit)
    {
        string searchKeyword = name.ToLower();

        Query query = _usersCollection
            .WhereGreaterThanOrEqualTo("NameLower", searchKeyword)
            .WhereLessThanOrEqualTo("NameLower", searchKeyword + "\uf8ff")
            .Limit(limit);
        
        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(doc => MapSnapshotToUser(doc));
    }

    async Task IUserRepository.SetActiveThemeAsync(string userId, string themeId)
    {
        DocumentReference docRef = _usersCollection.Document(userId);
        await docRef.UpdateAsync("ActiveThemeId", themeId);
    }

    async Task IUserRepository.UpdateCoinBalanceAsync(string userId, int amount)
    {
        DocumentReference docRef = _usersCollection.Document(userId);
        await docRef.UpdateAsync("CoinBalance", FieldValue.Increment(amount));
    }

    async Task IUserRepository.UpdateProfileAsync(string userId, string name, string? newPassword, string? avatarUrl, string? gender, string? birthday)
    {
        DocumentReference docRef = _usersCollection.Document(userId);
        var updatedData = new Dictionary<string, Object>
        {
            { "Name", name },
            { "NameLower", name.ToLower() },
            { "AvatarUrl", avatarUrl ?? "" },
            { "Gender", gender ?? "" },
            { "Birthday", birthday ?? " " },
            { "UpdatedAt", Timestamp.GetCurrentTimestamp() }
        };

        if (!string.IsNullOrEmpty(newPassword))
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
            updatedData.Add("HashPassword", hashedPassword);
        }
        await docRef.UpdateAsync(updatedData);
    }

    public async Task UpdateAsync(User user)
    {
        DocumentReference docRef = _usersCollection.Document(user.Id);
        
        var updatedData = new Dictionary<string, object>
        {
            { "Email", user.Email },
            { "Name", user.Name ?? "" },
            { "NameLower", (user.Name ?? "").ToLower() },
            { "HashPassword", user.HashPassword ?? "" },
            { "AvatarUrl", user.AvatarUrl ?? "" },
            { "Gender", user.Gender ?? "" },
            { "Birthday", user.Birthday ?? "" },
            { "CoinBalance", user.CoinBalance },
            { "ActiveThemeId", user.ActiveThemeId },
            { "AuthProvider", user.AuthProvider },
            { "UpdatedAt", Timestamp.GetCurrentTimestamp() }
        };

        if (!string.IsNullOrEmpty(user.ResetOtp) && user.OtpExpiry.HasValue)
        {
            // otp (forgot pasword), save DB
            updatedData["ResetOtp"] = user.ResetOtp;
            updatedData["OtpExpiry"] = Timestamp.FromDateTime(user.OtpExpiry.Value.ToUniversalTime());
        }
        else
        {
            updatedData["ResetOtp"] = FieldValue.Delete;
            updatedData["OtpExpiry"] = FieldValue.Delete;
        }

        await docRef.UpdateAsync(updatedData);
    }

    private User MapSnapshotToUser(DocumentSnapshot snapshot)
    {
        var user = new User
        {
            Id = snapshot.Id,
            Email = snapshot.GetValue<string>("Email") ?? string.Empty,
            Name = snapshot.GetValue<string>("Name") ?? "username",
            HashPassword = snapshot.GetValue<string>("HashPassword") ?? string.Empty,
            AvatarUrl = snapshot.ContainsField("AvatarUrl") ? snapshot.GetValue<string>("AvatarUrl") : "",
            Gender = snapshot.ContainsField("Gender") ? snapshot.GetValue<string>("Gender") : null,
            Birthday = snapshot.ContainsField("Birthday") ? snapshot.GetValue<string>("Birthday") : null,
            AuthProvider = snapshot.ContainsField("AuthProvider") ? snapshot.GetValue<string>("AuthProvider") : "Email",
            
            CoinBalance = snapshot.ContainsField("CoinBalance") ? Convert.ToInt32(snapshot.GetValue<long>("CoinBalance")) : 0,
            
            ActiveThemeId = snapshot.ContainsField("ActiveThemeId") 
                            ? snapshot.GetValue<string>("ActiveThemeId") 
                            : "default_theme_id",

            CreatedAt = snapshot.ContainsField("CreatedAt") ? snapshot.GetValue<DateTime>("CreatedAt") : DateTime.UtcNow,
            UpdatedAt = snapshot.ContainsField("UpdatedAt") ? snapshot.GetValue<DateTime>("UpdatedAt") : (DateTime?)null,
            
            ResetOtp = snapshot.ContainsField("ResetOtp") ? snapshot.GetValue<string>("ResetOtp") : null,
            OtpExpiry = snapshot.ContainsField("OtpExpiry") ? snapshot.GetValue<DateTime>("OtpExpiry") : (DateTime?)null
        };

        if (snapshot.ContainsField("Role") && snapshot.GetValue<object>("Role") != null)
        {
            var roleValue = snapshot.GetValue<object>("Role").ToString();
            
            if (Enum.TryParse<UserRole>(roleValue, true, out var roleEnum))
            {
                user.Role = roleEnum;
            }
            else
            {
                user.Role = UserRole.User;
            }
        }
        else
        {
            user.Role = UserRole.User;
        }

        return user;
    }
}
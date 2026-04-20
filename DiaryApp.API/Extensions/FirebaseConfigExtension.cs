using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using DiaryApp.Infrastructure.Configurations;

namespace DiaryApp.Api.Extensions;

public static class FirebaseConfigExtension
{
    public static void AddFirebaseAdminConfig(this IServiceCollection services, IConfiguration configuration)
    {
        var firebaseSettings = configuration.GetSection("Firebase").Get<FirebaseSettings>();

        if (firebaseSettings == null || string.IsNullOrEmpty(firebaseSettings.ServiceAccountPath))
        {
            throw new Exception("Not found Firebase in appsettings.json!");
        }

        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(firebaseSettings.ServiceAccountPath),
                ProjectId = firebaseSettings.ProjectId 
            });
        }
    }
}
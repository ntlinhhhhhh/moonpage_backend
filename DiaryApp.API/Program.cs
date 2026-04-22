using DiaryApp.Application.Interfaces;
using DiaryApp.Application.Services;
using DiaryApp.Infrastructure.Repositories;
using DiaryApp.Infrastructure.Data;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using DiaryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using DiaryApp.Application.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DiaryApp.Infrastructure.Configurations;
using DiaryApp.Infrastructure.Providers;
using Google.Cloud.Firestore;
using DiaryApp.Infrastructure.Messaging;
using DiaryApp.Infrastructure.Workers;
using Microsoft.AspNetCore.ResponseCompression;
using Google.Api.Gax;
using DiaryApp.API.Middlewares;
using DiaryApp.Application.Interfaces.Repositories;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    var emulatorHost = Environment.GetEnvironmentVariable("FIRESTORE_EMULATOR_HOST");
    if (!string.IsNullOrEmpty(emulatorHost))
    {
        Console.WriteLine($"[Mode] run with Firestore Emulator: {emulatorHost}");
    }
    else
    {
        Console.WriteLine("[Mode] Run with Firebase/Firestore Production (Cloud)");
    }
}

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<GoogleCloudSettings>(builder.Configuration.GetSection("GoogleCloud"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);
var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
var projectId = builder.Configuration["Firebase:ProjectId"] ?? "moodyfy-3f2dd";

// authentication & authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        RoleClaimType = ClaimTypes.Role 
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

string firebaseBase64 = Environment.GetEnvironmentVariable("FIREBASE_KEY_BASE64");
string googleCredentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

if (!string.IsNullOrEmpty(firebaseBase64))
{
    byte[] decodedBytes = Convert.FromBase64String(firebaseBase64);
    string decodedJson = Encoding.UTF8.GetString(decodedBytes);
    FirebaseAdmin.FirebaseApp.Create(new FirebaseAdmin.AppOptions()
    {
        Credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromJson(decodedJson)
    });
}
else if (!string.IsNullOrEmpty(googleCredentialsPath) && File.Exists(googleCredentialsPath))
{
    FirebaseAdmin.FirebaseApp.Create(new FirebaseAdmin.AppOptions()
    {
        Credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(googleCredentialsPath)
    });
    Console.WriteLine($"[Firebase] Đã nạp Service Account từ: {googleCredentialsPath}");
}

// --- 5. FIRESTORE DB REGISTRATION ---
builder.Services.AddSingleton<FirestoreDb>(provider =>
{
    var firestoreBuilder = new FirestoreDbBuilder
    {
        ProjectId = projectId,
        EmulatorDetection = EmulatorDetection.EmulatorOrProduction
    };

    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FIRESTORE_EMULATOR_HOST")) 
        && !string.IsNullOrEmpty(firebaseBase64))
    {
        byte[] decodedBytes = Convert.FromBase64String(firebaseBase64);
        firestoreBuilder.JsonCredentials = Encoding.UTF8.GetString(decodedBytes);
    }

    return firestoreBuilder.Build();
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "DiaryApp_";
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

// dependency injection
builder.Services.AddSingleton<FirestoreProvider>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IThemeRepository, ThemeRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IDailyLogRepository, DailyLogRepository>();
builder.Services.AddScoped<IMomentRepository, MomentRepository>();
builder.Services.AddScoped<IAppNotificationRepository, AppNotificationRepository>();
builder.Services.AddScoped<IStatisticsRepository, StatisticsRepository>();
builder.Services.AddHttpClient<IGoogleStorageService, GoogleStorageService>();
builder.Services.AddScoped<IUserStreakRepository, UserStreakRepository>();
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();
builder.Services.AddScoped<IMessageProducer, RabbitMQProducer>();
builder.Services.AddHostedService<ImageUploadWorker>();
builder.Services.AddHostedService<DatabaseTaskWorker>();
builder.Services.AddHostedService<StreakCleanupWorker>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IDailyLogService, DailyLogService>();
builder.Services.AddScoped<IMomentService, MomentService>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IGoogleAuthProvider, GoogleAuthProvider>();
builder.Services.AddScoped<IFirebaseNotificationService, FirebaseNotificationService>();
builder.Services.AddScoped<IAppNotificationService, AppNotificationService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();

// controllers & swagger
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options => { options.SuppressMapClientErrors = false; });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Bearer {your_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "oauth2", Name = "Bearer", In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

app.UseResponseCompression();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<TokenBlacklistMiddleware>();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<FirestoreDb>();
        Console.WriteLine("[Firestore] Initializing connection to Firestore");
        await db.Collection("Users").Limit(1).GetSnapshotAsync(); 
        Console.WriteLine("[Firestore] Database is ready");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Firestore] warm-up warning:: {ex.Message}");
    }
}

app.MapGet("/", () => Results.Ok("DiaryApp API is Online!"));
app.Run();
# Moonpage Backend

Moonpage is a comprehensive diary and mood tracking application backend built with .NET 8. It leverages Google Firebase/Firestore for data storage, Redis for caching, RabbitMQ for asynchronous tasks, and Google Cloud Storage for media assets.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker and Docker Compose](https://www.docker.com/)
- [Google Cloud / Firebase Project](https://console.firebase.google.com/)

## 1. Environment Variables

The application uses environment variables for configuration. You can set these in your system, in a `.env` file (if using a loader), or directly in `docker-compose.yml`.

### Google Cloud & Firebase

The backend uses Firestore for the database and Google Cloud Storage for media.

| Variable | Description | Example/Default |
| :--- | :--- | :--- |
| `GoogleCloud__ProjectId` | Google Cloud/Firebase Project ID. | `moodyfy-3f2dd` |
| `GoogleCloud__ClientId` | Google OAuth 2.0 Client ID. | `...apps.googleusercontent.com` |
| `GoogleCloud__StorageBucket` | GCS Bucket for media uploads. | `moodyfy-3f2dd.firebasestorage.app` |
| `GoogleCloud__ServiceAccountPath`| Path to the service account JSON file inside the container. | `/app/firebase-key.json` |
| `FIREBASE_KEY_BASE64` | Base64 encoded content of the service account JSON (Alternative to path). | Optional |
| `GOOGLE_APPLICATION_CREDENTIALS` | Path to Google credentials for SDKs. | `/app/firebase-key.json` |

### Authentication (JWT)

| Variable | Description | Example/Default |
| :--- | :--- | :--- |
| `JwtSettings__Secret` | Secret key used to sign JWT tokens. | Required (Strong string) |
| `JwtSettings__Issuer` | JWT Token Issuer. | `DiaryApp` |
| `JwtSettings__Audience` | JWT Token Audience. | `DiaryApp` |
| `JwtSettings__ExpiryMinutes` | Access token lifetime in minutes. | `43200` (30 days) |

### Redis Cache

| Variable | Description | Example/Default |
| :--- | :--- | :--- |
| `Redis__ConnectionString` | Connection string for Redis. | `localhost:6379` (or `diary-redis:6379`) |

### RabbitMQ Messaging

| Variable | Description | Example/Default |
| :--- | :--- | :--- |
| `RabbitMQSettings__Url` | AMQP connection URL for RabbitMQ. | `amqp://guest:guest@localhost:5672` |

### Email (SMTP)

| Variable | Description | Example/Default |
| :--- | :--- | :--- |
| `EmailSettings__Email` | Sender email address. | `example@gmail.com` |
| `EmailSettings__Password` | App-specific password for the email. | Required |
| `EmailSettings__Host` | SMTP Hostname. | `smtp.gmail.com` |
| `EmailSettings__Port` | SMTP Port. | `587` |

### Google OAuth (SSO)

| Variable | Description | Example/Default |
| :--- | :--- | :--- |
| `GoogleOAuth__ClientId` | Google Client ID for OAuth login. | Required |
| `GoogleOAuth__ClientSecret` | Google Client Secret for OAuth login. | Required |

---

## 2. Setup Service Account

The application requires a Google Service Account key to interact with Firebase and GCS.

1.  Generate a service account key from the [Firebase Console](https://console.firebase.google.com/).
2.  Save the JSON file as `firebase-key.json` in the `DiaryApp.API/` directory.
3.  Ensure the path matches `GoogleCloud__ServiceAccountPath` or set `FIREBASE_KEY_BASE64`.

---

## 3. Start Services

### Using Docker Compose (Recommended)

Start the entire stack (API, Redis, RabbitMQ):

```bash
docker compose up -d
```

The API will be available at `http://localhost:8000`.

### Local Development

If you want to run the API locally while using Docker for infrastructure:

1.  Start infrastructure:
    ```bash
    docker compose up -d redis rabbitmq
    ```
2.  Run the API:
    ```bash
    dotnet run --project DiaryApp.API/DiaryApp.API.csproj
    ```

---

## 4. API Modules Overview

The API is organized into several key modules:

- **Auth:** Registration, Login (Standard & Google), Password recovery.
- **User:** Profile management, Theme inventory, Search.
- **DailyLog:** Mood tracking, sleep hours, menstruation cycle, activities.
- **Activity:** Custom activity tags management.
- **Theme:** UI configurations and custom mood icon sets.
- **Moment:** Social sharing and user updates.
- **Notification:** In-app and Push notifications (FCM).

For a detailed list of endpoints and DTOs, refer to [api.md](./api.md) or visit the Swagger UI at `/swagger`.

## 5. Health Check

Verify if the API is online:

```text
GET http://localhost:8000/
```

Response: `"DiaryApp API is Online!"`

## 6. Production Reverse Proxy

In production, it is recommended to expose the API through a reverse proxy (like Nginx or Traefik) with HTTPS:

```text
api.yourdomain.com:443 -> 127.0.0.1:8000
```


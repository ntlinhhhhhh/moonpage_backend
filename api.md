# API INTEGRATION DOCUMENTATION - DIARYAPP (MOODIFY)

This document provides the complete list of API endpoints, Request/Response structures (DTOs), and data validation rules required for Frontend and Backend integration.

## 1. Base Configuration
* **Base URL:** `http://<server-domain>` (e.g., local development: `http://localhost:8000`)
* **Authentication:** Most APIs require user authentication. Attach the header `Authorization: Bearer <access_token>` (Token is obtained from the Login API).
* **Content-Type:** Default is `application/json`.

---

## 2. Auth Module
*Manages account lifecycle: Registration, login, and password recovery.*

### 2.1. Register Account
* **Endpoint:** `POST /api/auth/register`
* **Description:** Create a new user account.
* **Body:**
```json
{
  "email": "user@email.com", // Required, valid email format
  "password": "password123", // Required, minimum 6 characters
  "name": "Nguyen Van A"     // Required, length between 4 - 50 characters
}
```

### 2.2. Standard Login
* **Endpoint:** POST /api/auth/login
* **Description:** Authenticate user via email and password to receive an access token.

Body:

```json
JSON
{
  "email": "user@email.com", // Required
  "password": "password123"  // Required, minimum 6 characters
}
```

### 2.3. Google Login (SSO)
* **Endpoint:** POST /api/auth/google-login

* **Description:** Authenticate user using Google Single Sign-On.

Body:
```json
JSON
{
  "idToken": "google_jwt_token" // Optional (Retrieved from Google SDK on the Client side)
}
```

### 2.4. Verify OTP
* **Endpoint:** POST /api/auth/verify-otp

* **Description:** Verify the OTP code sent to the user's email.

Body:
```json
JSON
{
  "email": "user@email.com", // Required
  "otpCode": "123456"        // Required
}
```

### 2.5. Forgot Password (Request OTP)
* **Endpoint:** POST /api/auth/forgot-password

* **Description:** Request an OTP code to be sent to the user's email for password recovery.

Body:
```json
JSON
{
  "email": "user@email.com" // Required
}
```

### 2.6. Reset Password
* **Endpoint:** POST /api/auth/reset-password

* **Description:** Set a new password after successful OTP verification.

Body:
```json
JSON
{
  "email": "user@email.com",    // Required
  "resetToken": "token_string", // Required, obtained from the verify-otp step
  "newPassword": "newPassword"  // Required, minimum 6 characters
}
```

## 3. User Module
*Manages personal profile information and the user's theme inventory.*

### 3.1. Standard Endpoints

* `GET /api/users` - Retrieve a list of all users (Admin only).

* `GET /api/users/me` - Retrieve the profile of the currently authenticated user.

* `GET /api/users/search` - Search for users.

* `GET /api/users/me/themes` - Retrieve the list of themes owned by the current.

* `DELETE /api/users/{id}` - Delete a specific user by ID.

### 3.2. Update Profile
* **Endpoint:** PUT /api/users/me

* **Description:** Update current user's profile information.

Body:
```json
JSON
{
  "name": "New Name",           // Required, minimum 1 character
  "avatarUrl": "https://...",   // Optional (Nullable)
  "gender": "Male",             // Optional (Nullable)
  "birthday": "2000-01-01"      // Optional (Nullable), ISO Date string
}
```

### 3.3. Buy Theme
* **Endpoint:** POST /api/users/me/themes/buy

* **Description:** Purchase a new theme from the store.

Body:
```json
JSON
{
  "themeId": "theme_id_here", // Required
  "price": 150                // Integer (Int32)
}
```

### 3.4. Activate Theme
* **Endpoint:** PUT /api/users/me/themes/active

* **Description:** Set a specific owned theme as the active theme.

Body:
```json
JSON
{
  "themeId": "theme_id_here"  // Required
}
```

## 4. DailyLog Module
*Core feature: Recording daily moods, sleep hours, and cycle tracking.*

### 4.1. Standard Endpoints
* `GET /api/dailylogs/date/{date}` - Retrieve a daily log by specific date (Format: YYYY-MM-DD).

* `DELETE /api/dailylogs/date/{date}` - Delete a daily log by date.

* `GET /api/dailylogs/month/{yearMonth}` - Retrieve all logs for a specific month (Used for Calendar view, format: YYYY-MM).

* `GET /api/dailylogs/activity/{activityId}/month/{yearMonth}` - Filter logs by a specific activity in a given month.

* `GET /api/dailylogs/mood/{moodId}` - Filter logs by a specific mood level.

* `GET /api/dailylogs/menstruation` - Retrieve menstruation cycle data.

* `GET /api/dailylogs/search` - General search for daily logs.

## 4.2. Create / Update Daily Log
* **Endpoint:** POST /api/dailylogs

* **Description:** Create a new log for the day or update an existing one.

Body:
```
JSON
{
  "baseMoodId": 4,                    // Integer (Int32). Enum from 1 to 5 representing mood levels
  "date": "2024-04-20",               // Required. Date of the log
  "note": "Today was a good day",     // Optional. Journal note
  "sleepHours": 8.5,                  // Double. Number of hours slept
  "isMenstruation": false,            // Boolean. Indicates if the user is in menstruation cycle
  "menstruationPhase": "Ovulation",   // Optional. Current phase of the cycle
  "dailyPhotos": ["url1", "url2"],    // Array of Strings. Attached photo URLs
  "activityIds": ["act1", "act2"]     // Array of Strings. IDs of the activities performed
}
```

## 5. Activity Module
*Activity tags that users can attach to their Daily Logs.*

### 5.1. Standard Endpoints
* `GET /api/activities` - Retrieve all available activities.

* `GET /api/activities/category/{category}` - Retrieve activities filtered by category.

* `GET /api/activities/{id}` - Retrieve details of a specific activity.

* `DELETE /api/activities/{id}` - Delete an activity.

### 5.2. Create / Update Activity
* **Endpoints:**   `POST /api/activities or PUT /api/activities/{id}`

* **Description:** Create a new activity or update an existing one (Usually Admin).

Body:
```
JSON
{
  "name": "Reading",        // Required, minimum 1 character
  "iconUrl": "https://...", // Required, minimum 1 character
  "category": "Hobby"       // Required
}
```

## 6. Theme Module
*UI configurations and custom mood icon sets.*

### 6.1. Standard Endpoints
* `GET /api/themes` - Retrieve the list of available themes in the store.
* `GET /api/themes/{id}` - Retrieve details of a specific theme.
* `GET /api/themes/{id}/moods` - Retrieve the custom mood icon set associated with a specific theme.
* `DELETE /api/themes/{id}` - Delete a theme.

### 6.2. Create / Update Theme
* **Endpoints:** `POST /api/themes` or `PUT /api/themes/{id}`
* **Description:** Publish a new theme to the store or update an existing one.
* **Body:**
```json
{
  "id": "theme_01",              // Required
  "name": "Ocean Blue",          // Required
  "price": 200,                  // Integer (From 0 to 2147483647)
  "thumbnailUrl": "https://...", // Optional
  "backgroundUrl": "https://...",// Optional
  "isActive": true,              // Boolean
  "moods": [                     // Array of associated custom moods
    {
      "baseMoodId": 1,           // Required. Enum [1, 2, 3, 4, 5]
      "iconUrl": "https://...",  // Required
      "customName": "Very Sad"   // Optional
    }
  ]
}
```

7. Moment Module
Social sharing and moment management.

* `GET /api/moments/me` - Retrieve moments posted by the current user.

*  `GET /api/moments/user/{userId}` - Retrieve moments posted by a specific user.

* `GET /api/moments/{id}` - Retrieve details of a specific moment.

* `DELETE /api/moments/{id}` - Delete a moment by ID.

* `POST /api/moments` - Post a new moment (Ensure proper multipart/form-data handling if uploading images directly).


## 8. Notification Module
*In-app notifications and Push notification management.*

### 8.1. Management Endpoints
* `GET /api/notifications` - Retrieve the user's notification list.

* `PUT /api/notifications/{id}/read` - Mark a specific notification as "Read".

* `DELETE /api/notifications/{id}` - Delete a specific notification.

* `DELETE /api/notifications/all` - Delete all notifications for the current user.

### 8.2. Create In-App Notification
* **Endpoint:** POST /api/notifications

* **Description:** Create a system notification to be saved in the database.

Body:
```
JSON
{
  "userId": "user_123",   // Optional. Target user ID
  "title": "Alert",       // Optional
  "message": "Content",   // Optional
  "type": "System"        // Optional (e.g., System, Promo, Alert)
}
```

### 8.3. Send Push Notification
* **Endpoint:** POST /api/notifications/send

* **Description:** Trigger a push notification to a device (Integrates with FCM/APNs).

Body:
```
JSON
{
  "token": "device_fcm_token", // Optional
  "title": "Notification Title", // Optional
  "body": "Push content body"    // Optional
}
```
9. System (Health Check)
* `GET /` - Root endpoint to verify if the API server is up and running (Health Check).
# API INTEGRATION DOCUMENTATION - MOONPAGE (MOODIFY)

This document provides a comprehensive list of all API endpoints, their purposes, request/response structures, and authentication requirements.

## 1. Base Configuration
* **Base URL:** `http://<server-domain>` (e.g., local development: `http://localhost:8000`)
* **Authentication:** Most APIs require a Bearer Token. Include the header: `Authorization: Bearer <access_token>`.
* **Content-Type:** 
    * Most endpoints: `application/json`
    * Upload endpoints: `multipart/form-data`

---

## 2. Auth Module (`api/auth`)
*Manages user authentication, registration, and password recovery.*

### 2.1. Register Account
* **Endpoint:** `POST /api/auth/register`
* **Description:** Create a new user account.
* **Request Body:**
```json
{
  "email": "user@email.com",
  "password": "password123", // Min 6 chars
  "name": "User Name"        // 4-50 chars
}
```
* **Response (200 OK):** `AuthResponseDto`

### 2.2. Standard Login
* **Endpoint:** `POST /api/auth/login`
* **Description:** Authenticate with email and password.
* **Request Body:**
```json
{
  "email": "user@email.com",
  "password": "password123"
}
```
* **Response (200 OK):** `AuthResponseDto`

### 2.3. Google Login (SSO)
* **Endpoint:** `POST /api/auth/google-login`
* **Description:** Authenticate using Google ID Token.
* **Request Body:**
```json
{
  "idToken": "google_id_token"
}
```
* **Response (200 OK):** `AuthResponseDto`

### 2.4. Forgot Password
* **Endpoint:** `POST /api/auth/forgot-password`
* **Description:** Request an OTP for password reset.
* **Request Body:**
```json
{
  "email": "user@email.com"
}
```
* **Response (200 OK):** `{ "message": "..." }`

### 2.5. Verify OTP
* **Endpoint:** `POST /api/auth/verify-otp`
* **Description:** Verify the OTP sent via email.
* **Request Body:**
```json
{
  "email": "user@email.com",
  "otpCode": "123456"
}
```
* **Response (200 OK):** `VerifyOtpResponseDto` (Contains `resetToken`)

### 2.6. Reset Password
* **Endpoint:** `POST /api/auth/reset-password`
* **Description:** Set a new password using the reset token.
* **Request Body:**
```json
{
  "email": "user@email.com",
  "resetToken": "token_from_verify_otp",
  "newPassword": "new_secure_password"
}
```
* **Response (200 OK):** `{ "message": "..." }`

### 2.7. Logout
* **Endpoint:** `POST /api/auth/logout`
* **Auth Required:** Yes
* **Description:** Invalidate the current session/token.
* **Response (200 OK):** `{ "message": "..." }`

---

## 3. User Module (`api/users`)
*Manages user profiles, themes, and store interactions.*

### 3.1. Get All Users (Admin)
* **Endpoint:** `GET /api/users`
* **Auth Required:** Yes (Role: Admin)
* **Response (200 OK):** `List<UserProfileDto>`

### 3.2. Delete User (Admin)
* **Endpoint:** `DELETE /api/users/{id}`
* **Auth Required:** Yes (Role: Admin)
* **Response (200 OK):** `{ "message": "..." }`

### 3.3. Get My Profile
* **Endpoint:** `GET /api/users/me`
* **Auth Required:** Yes
* **Response (200 OK):** `UserProfileDto`

### 3.4. Update My Profile
* **Endpoint:** `PUT /api/users/me`
* **Auth Required:** Yes
* **Request Body:** `UpdateProfileRequestDto`
```json
{
  "name": "New Name",
  "avatarUrl": "Optional existing URL",
  "gender": "Male/Female/Other",
  "birthday": "YYYY-MM-DD"
}
```
* **Response (200 OK):** `{ "message": "..." }`

### 3.5. Update Avatar
* **Endpoint:** `PUT /api/users/me/avatar`
* **Auth Required:** Yes
* **Content-Type:** `multipart/form-data`
* **Request Form:**
    * `ImageFile`: File (The image to upload)
* **Response (202 Accepted):** `{ "message": "..." }`

### 3.6. Search Users
* **Endpoint:** `GET /api/users/search?name={name}&limit={limit}`
* **Auth Required:** Yes
* **Response (200 OK):** `List<UserSearchResponseDto>`

### 3.7. Get My Themes
* **Endpoint:** `GET /api/users/me/themes`
* **Auth Required:** Yes
* **Description:** Get list of Theme IDs owned by the user.
* **Response (200 OK):** `List<string>`

### 3.8. Buy Theme
* **Endpoint:** `POST /api/users/me/store/buy-theme`
* **Auth Required:** Yes
* **Request Body:**
```json
{
  "themeId": "theme_id",
  "price": 100
}
```
* **Response (200 OK):** `{ "success": true, "message": "..." }`

### 3.9. Buy Streak Freeze
* **Endpoint:** `POST /api/users/me/store/buy-freeze`
* **Auth Required:** Yes
* **Response (200 OK):** `{ "success": true, "message": "..." }`

### 3.10. Activate Theme
* **Endpoint:** `PUT /api/users/me/themes/active`
* **Auth Required:** Yes
* **Request Body:**
```json
{
  "themeId": "theme_id"
}
```
* **Response (200 OK):** `{ "message": "..." }`

---

## 4. DailyLog Module (`api/dailylogs`)
*Core module for mood and activity tracking.*

### 4.1. Upsert Daily Log
* **Endpoint:** `POST /api/dailylogs`
* **Auth Required:** Yes
* **Content-Type:** `multipart/form-data`
* **Description:** Create or update a log for a specific date.
* **Request Form:**
    * `BaseMoodId`: int (1-5)
    * `Date`: string (YYYY-MM-DD)
    * `Note`: string (Optional)
    * `SleepHours`: double
    * `IsMenstruation`: bool
    * `MenstruationPhase`: string (Optional)
    * `Steps`: int (Optional)
    * `MusicRecord`: string (Optional)
    * `DailyPhotos`: List<File>
    * `ActivityIds`: List<string>
* **Response (200 OK):** `{ "message": "..." }`

### 4.2. Get Log by Date
* **Endpoint:** `GET /api/dailylogs/date/{date}`
* **Description:** Date format: YYYY-MM-DD.
* **Response (200 OK):** `DailyLogResponseDto`

### 4.3. Get Logs by Month
* **Endpoint:** `GET /api/dailylogs/month/{yearMonth}`
* **Description:** Format: YYYY-MM.
* **Response (200 OK):** `List<DailyLogResponseDto>`

### 4.4. Get Logs by Activity
* **Endpoint:** `GET /api/dailylogs/activity/{activityId}/month/{yearMonth}`
* **Response (200 OK):** `List<DailyLogResponseDto>`

### 4.5. Get Logs by Mood
* **Endpoint:** `GET /api/dailylogs/mood/{moodId}`
* **Response (200 OK):** `List<DailyLogResponseDto>`

### 4.6. Filter by Menstruation
* **Endpoint:** `GET /api/dailylogs/menstruation?isMenstruation={bool}`
* **Response (200 OK):** `List<DailyLogResponseDto>`

### 4.7. Search Logs by Note
* **Endpoint:** `GET /api/dailylogs/search?keyword={text}`
* **Response (200 OK):** `List<DailyLogResponseDto>`

### 4.8. Delete Log
* **Endpoint:** `DELETE /api/dailylogs/date/{date}`
* **Response (200 OK):** `{ "message": "..." }`

---

## 5. Activity Module (`api/activities`)
*Manages activity tags.*

### 5.1. Get All Activities
* **Endpoint:** `GET /api/activities`
* **Response (200 OK):** `List<ActivityResponseDto>`

### 5.2. Get by Category
* **Endpoint:** `GET /api/activities/category/{category}`
* **Response (200 OK):** `List<ActivityResponseDto>`

### 5.3. Get Activity Details
* **Endpoint:** `GET /api/activities/{id}`
* **Response (200 OK):** `ActivityResponseDto`

### 5.4. Create Activity (Admin)
* **Endpoint:** `POST /api/activities`
* **Auth Required:** Yes (Role: Admin)
* **Request Body:** `ActivityRequestDto`
* **Response (201 Created):** `ActivityResponseDto`

### 5.5. Update Activity (Admin)
* **Endpoint:** `PUT /api/activities/{id}`
* **Auth Required:** Yes (Role: Admin)
* **Request Body:** `ActivityRequestDto`
* **Response (200 OK):** `{ "message": "..." }`

### 5.6. Delete Activity (Admin)
* **Endpoint:** `DELETE /api/activities/{id}`
* **Auth Required:** Yes (Role: Admin)
* **Response (200 OK):** `{ "message": "..." }`

---

## 6. Theme Module (`api/themes`)
*Manages UI themes and mood icons.*

### 6.1. Get All Active Themes
* **Endpoint:** `GET /api/themes`
* **Response (200 OK):** `List<ThemeResponseDto>`

### 6.2. Get Theme Details
* **Endpoint:** `GET /api/themes/{id}`
* **Response (200 OK):** `ThemeResponseDto` (Full details)

### 6.3. Get Theme Mood Icons
* **Endpoint:** `GET /api/themes/{id}/moods`
* **Response (200 OK):** `List<ThemeMoodResponseDto>`

### 6.4. Create Theme (Admin)
* **Endpoint:** `POST /api/themes`
* **Auth Required:** Yes (Role: Admin)
* **Request Body:** `CreateThemeRequestDto`
* **Response (201 Created):** `{ "message": "..." }`

### 6.5. Update Theme (Admin)
* **Endpoint:** `PUT /api/themes/{id}`
* **Auth Required:** Yes (Role: Admin)
* **Request Body:** `CreateThemeRequestDto`
* **Response (200 OK):** `{ "message": "..." }`

### 6.6. Delete Theme (Admin)
* **Endpoint:** `DELETE /api/themes/{id}`
* **Auth Required:** Yes (Role: Admin)
* **Response (200 OK):** `{ "message": "..." }`

---

## 7. Moment Module (`api/moments`)
*Social sharing of daily logs/photos.*

### 7.1. Create Moment
* **Endpoint:** `POST /api/moments`
* **Auth Required:** Yes
* **Content-Type:** `multipart/form-data`
* **Request Form:**
    * `DailyLogId`: string (Optional)
    * `ImageFile`: File (Required)
    * `Caption`: string (Optional)
    * `IsPublic`: bool
    * `CapturedAt`: DateTime (Optional)
* **Response (202 Accepted):** `MomentResponseDto`

### 7.2. Get Moment Details
* **Endpoint:** `GET /api/moments/{id}`
* **Response (200 OK):** `MomentResponseDto`

### 7.3. Get My Moments
* **Endpoint:** `GET /api/moments/me`
* **Response (200 OK):** `List<MomentResponseDto>`

### 7.4. Get User's Moments
* **Endpoint:** `GET /api/moments/user/{userId}`
* **Response (200 OK):** `List<MomentResponseDto>`

### 7.5. Delete Moment
* **Endpoint:** `DELETE /api/moments/{id}`
* **Response (200 OK):** `{ "message": "..." }`

---

## 8. Notification Module (`api/notifications`)
*In-app and push notifications.*

### 8.1. Send Push Notification (Dev/Test)
* **Endpoint:** `POST /api/notifications/send`
* **Request Body:** `PushNotificationRequestDto`
* **Response (200 OK):** `{ "Success": true, "MessageId": "..." }`

### 8.2. Create In-App Notification
* **Endpoint:** `POST /api/notifications`
* **Request Body:** `AppNotificationRequestDto`
* **Response (201 Created):** `{ "Success": true, "Data": ... }`

### 8.3. Get My Notifications
* **Endpoint:** `GET /api/notifications`
* **Auth Required:** Yes
* **Response (200 OK):** `{ "Success": true, "Data": List<AppNotificationResponseDto> }`

### 8.4. Mark as Read
* **Endpoint:** `PUT /api/notifications/{id}/read`
* **Response (204 No Content)**

### 8.5. Delete Notification
* **Endpoint:** `DELETE /api/notifications/{id}`
* **Response (204 No Content)**

### 8.6. Delete All My Notifications
* **Endpoint:** `DELETE /api/notifications/all`
* **Response (200 OK):** `{ "Success": true, "Message": "..." }`

---

## 9. Statistics Module (`api/statistics`)
*User data analytics.*

### 9.1. Get Stats Summary
* **Endpoint:** `GET /api/statistics/summary?year={int}&month={int}`
* **Auth Required:** Yes
* **Response (200 OK):** `UserStatsSummaryDto`
    * `TotalLogs`: int
    * `TotalPhotos`: int
    * `CurrentStreak`: int
    * `LongestStreak`: int
    * `MoodDistribution`: List of `{ Label, Count, Percentage }`
    * `MoodFlow`: List of `{ Date, MoodId }`
    * `BestActivities`: List of `{ ActivityId, ActivityName, IconUrl, AverageMoodScore, Occurrence }`

---

## 10. Data Structures (DTOs)

### AuthResponseDto
```json
{
  "token": "string",
  "userId": "string",
  "name": "string",
  "avatarUrl": "string?"
}
```

### UserProfileDto
```json
{
  "id": "string",
  "email": "string",
  "name": "string",
  "role": "string",
  "avatarUrl": "string?",
  "gender": "string?",
  "birthday": "string?",
  "coinBalance": "int",
  "activeThemeId": "string",
  "authProvider": "string",
  "createdAt": "DateTime"
}
```

### DailyLogResponseDto
```json
{
  "id": "string",
  "baseMoodId": "int?",
  "date": "string (YYYY-MM-DD)",
  "note": "string?",
  "sleepHours": "double",
  "isMenstruation": "bool",
  "menstruationPhase": "string?",
  "steps": "int",
  "musicRecord": "string?",
  "dailyPhotos": ["string (URLs)"],
  "activityIds": ["string (IDs)"],
  "createdAt": "DateTime"
}
```

# Global success response:

## Response body:

- Endpoint-specific

## Status codes:

- [200 OK] - Operation performed successfully
- [201 Created] - Resource created
- [202 Accepted] - Request accepted & admitted for processing
- [204 No Content] - Operation successful, no content returned

# Global error responses:

## Response body:

```json
{
  "message": "<endpoint specific error message>"
}
```

## Status codes:

- [400 Bad Request] - Input validation failed
- [401 Unauthorized] - User is not authorized for operation
- [403 Forbidden] - User doesn't satisfy conditions for operation
- [404 Not Found] - Resource not found
- [409 Conflict] - Data update is conflicting with existing data
- [500 Internal Server Error] - Unexpected server error occurred

---

# Auth Endpoints:

## Register Account

- Endpoint:

```text
POST /api/auth/register
```

- Description: Creates a new user account.
- Auth required: No

### Request body (application/json):

- email (string, Required): Valid email format.
- password (string, Required): Minimum 6 characters.
- name (string, Required): Between 4 and 50 characters.

```json
{
  "email": "user@email.com",
  "password": "password123",
  "name": "Nguyen Van A"
}
```

### Responses:

- [200 OK] - Account created successfully.

```json
{
  "token": "eyJhbGciOiJIUzI1Ni...",
  "userId": "user_uuid_here",
  "name": "Nguyen Van A",
  "avatarUrl": null
}
```

## Login

- Endpoint:

```text
POST /api/auth/login
```

- Description: Authenticates a user and returns an access token.
- Auth required: No

### Request body (application/json):

- email (string, Required)
- password (string, Required)

```json
{
  "email": "user@email.com",
  "password": "password123"
}
```

### Responses:

- [200 OK] - Signed in successfully.

```json
{
  "token": "eyJhbGciOiJIUzI1Ni...",
  "userId": "user_uuid_here",
  "name": "Nguyen Van A",
  "avatarUrl": "https://..."
}
```

## Google Login

- Endpoint:

```text
POST /api/auth/google-login
```

- Description: Authenticates a user using a Google ID token.
- Auth required: No

### Request body (application/json):

- idToken (string, Required): The token from Google SDK.

```json
{
  "idToken": "google_jwt_token_here"
}
```

### Responses:

- [200 OK] - Signed in with Google successfully.

```json
{
  "token": "eyJhbGciOiJIUzI1Ni...",
  "userId": "user_uuid_here",
  "name": "Google User",
  "avatarUrl": "https://..."
}
```

## Verify OTP

- Endpoint:

```text
POST /api/auth/verify-otp
```

- Description: Verifies the OTP code sent to email.
- Auth required: No

### Request body (application/json):

- email (string, Required)
- otpCode (string, Required)

```json
{
  "email": "user@email.com",
  "otpCode": "123456"
}
```

### Responses:

- [200 OK] - OTP verified.

```json
{
  "resetToken": "temp_reset_token_here"
}
```

## Request password reset (Forgot Password)

- Endpoint:

```text
POST /api/auth/forgot-password
```

- Description: Sends a 6-digit OTP code to the user's email for password recovery.
- Auth required: No

### Request body (application/json):

- email (string, Required)

```json
{
  "email": "user@email.com"
}
```

### Responses:

- [200 OK] - OTP sent successfully.

```json
{
  "message": "We've sent an OTP to your email. Please check your email!"
}
```

## Reset Password

- Endpoint:

```text
POST /api/auth/reset-password
```

- Description: Sets a new password using a reset token previously verified.
- Auth required: No

### Request body (application/json):

- email (string, Required)
- resetToken (string, Required): Obtained from verify-otp.
- newPassword (string, Required): Minimum 6 characters.

```json
{
  "email": "user@email.com",
  "resetToken": "temp_reset_token_here",
  "newPassword": "newSecurePassword123"
}
```

### Responses:

- [200 OK] - Password updated successfully.

```json
{
  "message": "Password reset successful!"
}
```

## Logout

- Endpoint:

```text
POST /api/auth/logout
```

- Description: Invalidates the current session token.
- Auth required: Yes

### Responses:

- [200 OK] - Logged out successfully.

```json
{
  "message": "Logged out successfully!"
}
```

---

# User Endpoints:

## Get all users (Admin Only)

- Endpoint:

```text
GET /api/users
```

- Description: Retrieves a list of all users. Accessible only by Admin.
- Auth required: Yes (Role: Admin)

### Responses:

- [200 OK] - List of users retrieved.

```json
[
  {
    "id": "user_id_1",
    "email": "admin@example.com",
    "name": "Admin User",
    "role": "Admin",
    "avatarUrl": "https://..."
  },
  {
    "id": "user_id_2",
    "email": "user@example.com",
    "name": "Normal User",
    "role": "User",
    "avatarUrl": null
  }
]
```

## Delete user (Admin Only)

- Endpoint:

```text
DELETE /api/users/:id
```

- Description: Deletes a specific user by ID. Accessible only by Admin.
- Auth required: Yes (Role: Admin)

### Responses:

- [200 OK] - User deleted successfully.

```json
{
  "message": "User deleted successfully."
}
```

## Get current user profile

- Endpoint:

```text
GET /api/users/me
```

- Description: Retrieves the profile of the authenticated user.
- Auth required: Yes

### Responses:

- [200 OK] - Profile retrieved.

```json
{
  "id": "user_id",
  "email": "user@example.com",
  "name": "Nguyen Van A",
  "role": "User",
  "avatarUrl": "https://...",
  "gender": "Male",
  "birthday": "2000-01-01",
  "coinBalance": 500,
  "activeThemeId": "theme_01",
  "authProvider": "Password",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

## Update profile

- Endpoint:

```text
PUT /api/users/me
```

- Description: Updates basic profile information for the authenticated user.
- Auth required: Yes

### Request body (application/json):

- name (string, Required)
- gender (string, Optional)
- birthday (string, Optional): YYYY-MM-DD format.

```json
{
  "name": "New Name",
  "gender": "Male",
  "birthday": "2000-01-01"
}
```

### Responses:

- [200 OK] - Profile updated.

```json
{
  "message": "Your profile has been updated successfully!"
}
```

## Update avatar

- Endpoint:

```text
PUT /api/users/me/avatar
```

- Description: Uploads a new avatar image for the authenticated user.
- Auth required: Yes

### Request body (multipart/form-data):

- ImageFile (File, Required): The image file to upload.

### Responses:

- [202 Accepted] - Upload accepted and is being processed.

```json
{
  "message": "Avatar is being processed."
}
```

## Delete my account

- Endpoint:

```text
DELETE /api/users/me
```

- Description: Deletes the authenticated user's account.
- Auth required: Yes

### Responses:

- [200 OK] - Account deleted successfully.

```json
{
  "message": "Your account has been deleted successfully."
}
```

## Search users

- Endpoint:

```text
GET /api/users/search
```

- Description: Searches for users by name.
- Auth required: Yes

### Query parameters:

- name (string, Required): The search keyword.
- limit (int, Required): Max results to return.

### Responses:

- [200 OK] - Search results.

```json
[
  {
    "id": "user_id_1",
    "name": "Nguyen Van A",
    "avatarUrl": "https://...",
    "email": "user1@example.com"
  }
]
```

## Get my owned themes

- Endpoint:

```text
GET /api/users/me/themes
```

- Description: Retrieves a list of theme IDs owned by the authenticated user.
- Auth required: Yes

### Responses:

- [200 OK] - List of theme IDs.

```json
["theme_01", "theme_02"]
```

## Buy theme

- Endpoint:

```text
POST /api/users/me/store/buy-theme
```

- Description: Purchases a theme from the store using coins.
- Auth required: Yes

### Request body (application/json):

- themeId (string, Required)
- price (int, Required)

```json
{
  "themeId": "theme_ocean",
  "price": 200
}
```

### Responses:

- [200 OK] - Theme purchased successfully.

```json
{
  "success": true,
  "message": "Theme purchased successfully!"
}
```

## Buy streak freeze

- Endpoint:

```text
POST /api/users/me/store/buy-freeze
```

- Description: Purchases a streak freeze item using coins.
- Auth required: Yes

### Responses:

- [200 OK] - Streak freeze purchased successfully.

```json
{
  "success": true,
  "message": "Streak freeze purchased successfully!"
}
```

## Change active theme

- Endpoint:

```text
PUT /api/users/me/themes/active
```

- Description: Changes the currently applied theme for the authenticated user.
- Auth required: Yes

### Request body (application/json):

- themeId (string, Required)

```json
{
  "themeId": "theme_ocean"
}
```

### Responses:

- [200 OK] - Theme changed successfully.

```json
{
  "message": "Your new theme has been applied successfully!"
}
```

---

# Daily Log Endpoints:

## Upsert daily log

- Endpoint:

```text
POST /api/dailylogs
```

- Description: Creates or updates a log for a specific date.
- Auth required: Yes

### Request body (multipart/form-data):

- Date (string, Required): YYYY-MM-DD.
- BaseMoodId (int, Optional): 1 (Very Sad) to 5 (Very Happy).
- Note (string, Optional).
- SleepHours (double, Optional).
- IsMenstruation (bool, Optional).
- MenstruationPhase (string, Optional).
- Steps (int, Optional).
- MusicRecord (string, Optional).
- DailyPhotos (Files, Optional): Multiple image files.
- ActivityIds (strings, Optional): List of activity IDs.

### Responses:

- [200 OK] - Log saved successfully.

```json
{
  "message": "Your log was saved successfully!"
}
```

## Get log by date

- Endpoint:

```text
GET /api/dailylogs/date/:date
```

- Description: Retrieves log for a specific date (YYYY-MM-DD).
- Auth required: Yes

### Responses:

- [200 OK] - Log retrieved.

```json
{
  "id": "log_id",
  "baseMoodId": 4,
  "date": "2024-04-20",
  "yearMonth": "2024-04",
  "note": "Great day!",
  "sleepHours": 8.0,
  "isMenstruation": false,
  "menstruationPhase": null,
  "steps": 10000,
  "musicRecord": "Classical",
  "dailyPhotos": ["https://storage.../image1.jpg"],
  "activityIds": ["act_sport", "act_reading"],
  "createdAt": "2024-04-20T10:00:00Z",
  "updatedAt": "2024-04-20T10:05:00Z"
}
```

## Get logs by month

- Endpoint:

```text
GET /api/dailylogs/month/:yearMonth
```

- Description: Retrieves all logs for a specific month (YYYY-MM).
- Auth required: Yes

### Responses:

- [200 OK] - List of logs.

```json
[
  {
    "id": "log_id_1",
    "baseMoodId": 4,
    "date": "2024-04-20",
    "yearMonth": "2024-04",
    "note": "...",
    "sleepHours": 7.5,
    "isMenstruation": false,
    "menstruationPhase": "",
    "steps": 8000,
    "musicRecord": "",
    "dailyPhotos": [],
    "activityIds": [],
    "createdAt": "...",
    "updatedAt": "..."
  }
]
```

## Get logs by activity

- Endpoint:

```text
GET /api/dailylogs/activity/:activityId/month/:yearMonth
```

- Description: Retrieves logs containing a specific activity for a month.
- Auth required: Yes

### Responses:

- [200 OK] - List of logs.

## Get logs by mood

- Endpoint:

```text
GET /api/dailylogs/mood/:moodId
```

- Description: Retrieves all logs with a specific mood ID.
- Auth required: Yes

### Responses:

- [200 OK] - List of logs.

## Get logs by menstruation

- Endpoint:

```text
GET /api/dailylogs/menstruation
```

- Description: Filters logs based on menstruation status.
- Auth required: Yes

### Query parameters:

- isMenstruation (bool, Required).

### Responses:

- [200 OK] - List of logs.

## Search logs by note

- Endpoint:

```text
GET /api/dailylogs/search
```

- Description: Searches through log notes using a keyword.
- Auth required: Yes

### Query parameters:

- keyword (string, Required).

### Responses:

- [200 OK] - List of logs.

## Delete log by date

- Endpoint:

```text
DELETE /api/dailylogs/date/:date
```

- Description: Deletes the log for a specific date.
- Auth required: Yes

### Responses:

- [200 OK] - Log deleted successfully.

```json
{
  "message": "Your log has been deleted successfully."
}
```

---

# Moment Endpoints:

## Create moment

- Endpoint:

```text
POST /api/moments
```

- Description: Shares a moment (photo + caption) to the social feed.
- Auth required: Yes

### Request body (multipart/form-data):

- ImageFile (File, Required): The photo to share.
- DailyLogId (string, Optional): Associated daily log ID.
- Caption (string, Optional).
- IsPublic (bool, Optional).
- CapturedAt (DateTime, Optional): Default is current UTC.

### Responses:

- [202 Accepted] - Moment creation accepted.

```json
{
  "id": "moment_id",
  "userId": "user_id",
  "userName": "Nguyen Van A",
  "userAvatarUrl": "https://...",
  "imageUrl": "https://...",
  "caption": "A nice view!",
  "isPublic": true,
  "capturedAt": "2024-04-20T10:00:00Z"
}
```

## Get moment by ID

- Endpoint:

```text
GET /api/moments/:id
```

- Description: Retrieves details of a specific moment.
- Auth required: Yes

### Responses:

- [200 OK] - Moment details.

## Get my moments

- Endpoint:

```text
GET /api/moments/me
```

- Description: Retrieves all moments posted by the authenticated user.
- Auth required: Yes

### Responses:

- [200 OK] - List of moments.

## Get moments by user ID

- Endpoint:

```text
GET /api/moments/user/:userId
```

- Description: Retrieves moments posted by a specific user.
- Auth required: Yes

### Responses:

- [200 OK] - List of moments.

## Delete moment

- Endpoint:

```text
DELETE /api/moments/:id
```

- Description: Deletes a specific moment.
- Auth required: Yes

### Responses:

- [204 No Content] - Success.

---

# Notification Endpoints:

## Send push notification (Dev/Admin Only)

- Endpoint:

```text
POST /api/notifications/push
```

- Description: Sends a push notification using FCM token.
- Auth required: Yes

### Request body (application/json):

- token (string, Required): Device FCM token.
- title (string, Required).
- body (string, Required).
- imageUrl (string, Optional): Large image URL for the notification.

```json
{
  "token": "fcm_token_here",
  "title": "Hello!",
  "body": "This is a test notification.",
  "imageUrl": "https://example.com/img.png"
}
```

### Responses:

- [200 OK] - Notification sent.

```json
{
  "success": true,
  "messageId": "..."
}
```

## Create in-app notification

- Endpoint:

```text
POST /api/notifications/in-app
```

- Description: Creates a new in-app notification record.
- Auth required: Yes

### Request body (application/json):

- userId (string, Required)
- title (string, Required)
- message (string, Required)
- type (string, Optional): Default is "System".

```json
{
  "userId": "target_user_id",
  "title": "Welcome!",
  "message": "Thanks for joining us.",
  "type": "System"
}
```

### Responses:

- [201 Created] - Notification created.

```json
{
  "success": true,
  "data": {
    "id": "...",
    "title": "...",
    "message": "...",
    "type": "...",
    "isRead": false,
    "createdAt": "..."
  }
}
```

## Get my notifications

- Endpoint:

```text
GET /api/notifications/me
```

- Description: Retrieves a list of in-app notifications for the authenticated user.
- Auth required: Yes

### Responses:

- [200 OK] - List of notifications.

```json
{
  "success": true,
  "data": [
    {
      "id": "...",
      "title": "...",
      "message": "...",
      "type": "...",
      "isRead": false,
      "createdAt": "..."
    }
  ]
}
```

## Mark notification as read

- Endpoint:

```text
PUT /api/notifications/:id/read
```

- Description: Marks a specific notification as read.
- Auth required: Yes

### Responses:

- [204 No Content] - Success.

## Delete notification

- Endpoint:

```text
DELETE /api/notifications/:id
```

- Description: Deletes a specific notification.
- Auth required: Yes

### Responses:

- [204 No Content] - Success.

## Delete all my notifications

- Endpoint:

```text
DELETE /api/notifications/all
```

- Description: Clears all notifications for the authenticated user.
- Auth required: Yes

### Responses:

- [200 OK] - All notifications deleted.

```json
{
  "success": true,
  "message": "All your notifications have been cleared!"
}
```

---

# Statistics Endpoints:

## Get user statistics summary

- Endpoint:

```text
GET /api/statistics/summary
```

- Description: Retrieves user statistics including streaks and mood trends.
- Auth required: Yes

### Query parameters:

- year (int, Optional): Year for statistics. Defaults to current year.
- month (int, Optional): Month for statistics.

### Responses:

- [200 OK] - Statistics retrieved.

```json
{
  "totalLogs": 45,
  "totalPhotos": 12,
  "currentStreak": 5,
  "longestStreak": 10,
  "moodDistribution": [
    { "baseMoodId": 5, "count": 20, "percentage": 44.4 }
  ],
  "moodFlow": [
    { "date": "2024-04-20", "moodId": 4 }
  ],
  "influenceActivities": [
    { "activityId": "act_1", "activityName": "Reading", "averageMoodScore": 4.8, "occurrence": 5 }
  ]
}
```

---

# Activity Endpoints:

## Get all activities

- Endpoint:

```text
GET /api/activities
```

- Description: Lists all available activities, ordered by Name.
- Auth required: Yes

### Responses:

- [200 OK] - List of activities.

## Get activities by category

- Endpoint:

```text
GET /api/activities/category/:category
```

- Description: Filters activities by category name.
- Auth required: Yes

### Responses:

- [200 OK] - List of activities.

## Get activity by ID

- Endpoint:

```text
GET /api/activities/:id
```

- Description: Retrieves details of a specific activity.
- Auth required: Yes

### Responses:

- [200 OK] - Activity details.

## Create activity (Admin Only)

- Endpoint:

```text
POST /api/activities
```

- Description: Creates a new activity. Accessible only by Admin.
- Auth required: Yes (Role: Admin)

### Request body (application/json):

- name (string, Required)
- iconUrl (string, Required)
- category (string, Optional)

### Responses:

- [201 Created] - Activity created.

## Update activity (Admin Only)

- Endpoint:

```text
PUT /api/activities/:id
```

- Description: Updates an existing activity. Accessible only by Admin.
- Auth required: Yes (Role: Admin)

### Responses:

- [200 OK] - Activity updated.

## Delete activity (Admin Only)

- Endpoint:

```text
DELETE /api/activities/:id
```

- Description: Deletes an activity. Accessible only by Admin.
- Auth required: Yes (Role: Admin)

### Responses:

- [200 OK] - Activity deleted.

---

# Theme Endpoints:

## Get all active themes

- Endpoint:

```text
GET /api/themes
```

- Description: Lists all themes available in the store.
- Auth required: Yes

### Responses:

- [200 OK] - List of themes.

## Get theme by ID

- Endpoint:

```text
GET /api/themes/:id
```

- Description: Retrieves details of a specific theme.
- Auth required: Yes

### Responses:

- [200 OK] - Theme details.

## Get theme mood icons

- Endpoint:

```text
GET /api/themes/:id/moods
```

- Description: Retrieves custom mood icons associated with a theme.
- Auth required: Yes

### Responses:

- [200 OK] - List of mood icons.

## Create theme (Admin Only)

- Endpoint:

```text
POST /api/themes
```

- Description: Creates a new theme. Accessible only by Admin.
- Auth required: Yes (Role: Admin)

### Request body (application/json):

- id (string, Required): Unique theme ID.
- name (string, Required)
- price (int, Required)
- thumbnailUrl (string, Optional)
- backgroundUrl (string, Optional)
- isActive (bool, Optional): Default is true.
- moods (array, Required): List of mood icons.

```json
{
  "id": "theme_summer",
  "name": "Summer Vibe",
  "price": 300,
  "moods": [
    { "baseMoodId": 5, "iconUrl": "https://...", "customName": "Sunshine" }
  ]
}
```

### Responses:

- [201 Created] - Theme created.

## Update theme (Admin Only)

- Endpoint:

```text
PUT /api/themes/:id
```

- Description: Updates an existing theme. Accessible only by Admin.
- Auth required: Yes (Role: Admin)

### Responses:

- [200 OK] - Theme updated.

## Delete theme (Admin Only)

- Endpoint:

```text
DELETE /api/themes/:id
```

- Description: Deletes a theme. Accessible only by Admin.
- Auth required: Yes (Role: Admin)

### Responses:

- [200 OK] - Theme deleted.

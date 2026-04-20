namespace DiaryApp.Application.Interfaces; 

public interface IFirebaseNotificationService
{
    Task<string> SendPushNotificationAsync(string deviceToken, string title, string body);
}
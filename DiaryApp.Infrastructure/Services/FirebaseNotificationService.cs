using FirebaseAdmin.Messaging;
using DiaryApp.Application.Interfaces; 

namespace DiaryApp.Infrastructure.Services;

public class FirebaseNotificationService : IFirebaseNotificationService
{
    public async Task<string> SendPushNotificationAsync(string deviceToken, string title, string body)
    {
        var message = new Message
        {
            Token = deviceToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            }
        };

        return await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }
}
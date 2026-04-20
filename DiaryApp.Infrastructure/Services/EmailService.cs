using DiaryApp.Application.Interfaces;
using DiaryApp.Infrastructure.Configurations;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DiaryApp.Infrastructure.Services;

public class EmailService(IOptions<EmailSettings> emailSettings) : IEmailService
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;

    async Task IEmailService.SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress("Dairy Support", _emailSettings.Email));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        email.Body = bodyBuilder.ToMessageBody();

        var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls); // connect
            await client.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password); // login email
            await client.SendAsync(email); // send

        } catch (Exception ex)
        {
            throw new Exception($"Lỗi gửi Email: {ex.Message}");
        } finally
        {
            await client.DisconnectAsync(true);
        }
        
    }

}
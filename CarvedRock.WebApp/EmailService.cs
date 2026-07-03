using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CarvedRock.WebApp;

public class EmailService : IEmailSender
{
    private readonly SmtpClient _client;
    public EmailService(IConfiguration config)
    {
        var smtpServer = config.GetValue<string>("CarvedRock:SmtpServer")!;
        if (smtpServer.StartsWith("tcp://"))
            smtpServer = smtpServer[6..];

        var parsedServer = smtpServer.Split(':');
        var host = parsedServer[0];
        var port = int.Parse(parsedServer[1]);
        _client = new() { Port = port, Host = host };
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {                
        var mailMessage = new MailMessage
        {
            Body = htmlMessage,
            Subject = subject,
            IsBodyHtml = true,
            From = new MailAddress("e-commerce@carvedrock.com", "Carved Rock Shop"),
            To = { email }
        };
        return _client.SendMailAsync(mailMessage);
    }
}

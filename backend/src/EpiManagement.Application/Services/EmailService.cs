using EpiManagement.Domain.Entities;
using MailKit.Net.Smtp;
using MimeKit;

namespace EpiManagement.Application.Services;

public class EmailService
{
    public async Task SendAsync(SystemConfig config, string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(config.SmtpFromName, config.SmtpFromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(config.SmtpHost, config.SmtpPort, config.SmtpUseSsl, ct);
        await client.AuthenticateAsync(config.SmtpUser, config.SmtpPassword, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}

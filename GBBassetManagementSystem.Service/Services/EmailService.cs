using GBBassetManagementSystem.Service.Interfaces;
using GBBassetManagementSystem.Shared.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GBBassetManagementSystem.Service.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlMessage)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException(
                "Recipient email address cannot be empty.",
                nameof(toEmail));
        }

        MimeMessage email = new();

        email.From.Add(
            new MailboxAddress(
                _emailSettings.SenderName,
                _emailSettings.SenderEmail));

        email.To.Add(MailboxAddress.Parse(toEmail));

        email.Subject = subject;

        BodyBuilder bodyBuilder = new()
        {
            HtmlBody = htmlMessage
        };

        email.Body = bodyBuilder.ToMessageBody();
        
using SmtpClient smtpClient = new();

// Disable certificate revocation checking because the local
// certificate chain is currently failing the revocation check.
smtpClient.CheckCertificateRevocation = false;

SecureSocketOptions socketOptions =
    _emailSettings.EnableSsl
        ? SecureSocketOptions.StartTls
        : SecureSocketOptions.None;

Console.WriteLine("Connecting to Gmail SMTP...");
        await smtpClient.ConnectAsync(
            _emailSettings.SmtpServer,
            _emailSettings.Port,
            socketOptions);
Console.WriteLine("Connected successfully.");
        await smtpClient.AuthenticateAsync(
            _emailSettings.Username,
            _emailSettings.Password);
Console.WriteLine("Authenticated.");
        await smtpClient.SendAsync(email);

        await smtpClient.DisconnectAsync(true);
    }
}
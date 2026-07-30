using Microsoft.Extensions.Options;
using FluyoV2.Settings;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Logging;

namespace FluyoV2.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("Recipient email is required", nameof(toEmail));

        using var message = new MailMessage();
        message.From = new MailAddress(_settings.FromEmail, _settings.FromName);
        message.To.Add(new MailAddress(toEmail));
        message.Subject = subject;
        message.Body = htmlBody;
        message.IsBodyHtml = true;

        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Timeout = 10000
        };

        if (!string.IsNullOrEmpty(_settings.SmtpUser))
        {
            client.Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPass);
        }

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            // Log full exception with stack trace and inner exceptions
            _logger.LogError(ex, "Failed to send email to {ToEmail} via SMTP {Host}:{Port}", toEmail, _settings.SmtpHost, _settings.SmtpPort);

            // Throw a concise error message (details are in logs)
            throw new Exception("Failure sending mail.");
        }
    }
}

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Zetflix.Models;

namespace Zetflix.Services;

public interface IEmailService
{
    Task<bool> SendAsync(string toEmail, string subject, string htmlBody);
}

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string toEmail, string subject, string htmlBody)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Email sending is disabled. Message to {Email} with subject {Subject} was skipped.", toEmail, subject);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_settings.SmtpHost) ||
            string.IsNullOrWhiteSpace(_settings.SmtpUser) ||
            string.IsNullOrWhiteSpace(_settings.SmtpPassword) ||
            string.IsNullOrWhiteSpace(_settings.FromAddress))
        {
            _logger.LogWarning("Email settings are incomplete. Message to {Email} was not sent.", toEmail);
            return false;
        }

        using var message = new MailMessage(
            from: new MailAddress(_settings.FromAddress, _settings.FromName),
            to: new MailAddress(toEmail))
        {
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        using var smtpClient = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPassword)
        };

        try
        {
            await smtpClient.SendMailAsync(message);
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not send email to {Email}.", toEmail);
            return false;
        }
    }
}

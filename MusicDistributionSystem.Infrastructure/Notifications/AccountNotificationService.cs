using Microsoft.Extensions.Options;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Domain.Contracts.Logging;
using MusicDistributionSystem.Infrastructure.Configuration;
using System.Net;
using System.Net.Mail;

namespace MusicDistributionSystem.Infrastructure.Notifications
{
    public class AccountNotificationService : IAccountNotificationService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IAppLogger _appLogger;

        public AccountNotificationService(
            IOptions<EmailSettings> emailSettings,
            IAppLogger appLogger)
        {
            _emailSettings = emailSettings.Value;
            _appLogger = appLogger;
        }

        public Task SendVerificationCodeAsync(string destination, string code)
        {
            return SendEmailAsync(
                destination,
                "Verify your SoundSphere account",
                $"""
                <p>Hello,</p>
                <p>Your SoundSphere verification code is:</p>
                <h2 style="letter-spacing: 4px;">{code}</h2>
                <p>This code expires in 10 minutes.</p>
                <p>If you did not create this account, you can ignore this email.</p>
                """);
        }

        public Task SendPasswordResetCodeAsync(string destination, string code)
        {
            return SendEmailAsync(
                destination,
                "Reset your SoundSphere password",
                $"""
                <p>Hello,</p>
                <p>Your SoundSphere password reset code is:</p>
                <h2 style="letter-spacing: 4px;">{code}</h2>
                <p>This code expires in 15 minutes.</p>
                <p>If you did not request a password reset, you can ignore this email.</p>
                """);
        }

        private async Task SendEmailAsync(string destination, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(_emailSettings.SenderEmail) ||
                string.IsNullOrWhiteSpace(_emailSettings.Username) ||
                string.IsNullOrWhiteSpace(_emailSettings.Password))
            {
                await _appLogger.LogWarningAsync("AccountNotification",
                    $"SMTP not configured. Would send to '{destination}' with subject '{subject}'. " +
                    "Set EmailSettings:Password via User Secrets.");
                return;
            }

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                message.To.Add(destination);

                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
                };

                await client.SendMailAsync(message);
                await _appLogger.LogInformationAsync("AccountNotification",
                    $"Email sent to '{destination}' with subject '{subject}'.");
            }
            catch (Exception ex)
            {
                await _appLogger.LogErrorAsync("AccountNotification",
                    $"Failed to send email to '{destination}' with subject '{subject}': {ex.Message}", ex);
            }
        }
    }
}
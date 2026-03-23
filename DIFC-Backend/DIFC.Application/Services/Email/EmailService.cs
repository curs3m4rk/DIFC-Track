using DIFC.Application.Interfaces.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace DIFC.Application.Services.Email
{
    /// <summary>
    /// Sends transactional emails via SMTP using MailKit.
    ///
    /// WHY a separate EmailService and not inline in AuthService?
    /// Single Responsibility Principle. AuthService handles auth logic.
    /// EmailService handles email delivery. Tomorrow if you switch
    /// from Gmail SMTP to SendGrid, you only change this one file.
    ///
    /// WHY MimeKit/MailKit over System.Net.Mail?
    /// System.Net.Mail.SmtpClient is deprecated by Microsoft.
    /// MailKit is async-first, actively maintained, supports OAuth2,
    /// and handles modern TLS correctly — it's the .NET standard.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink)
        {
            var settings = _config.GetSection("EmailSettings");

            // ── Build the email ──────────────────────────────────────────
            // MimeMessage is the email "envelope + letter"
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                settings["SenderName"], settings["SenderEmail"]));
            message.To.Add(new MailboxAddress(fullName, toEmail));
            message.Subject = "Password Reset Request — DIFC";

            // ── Email body (plain text + HTML) ───────────────────────────
            // WHY both? Email clients vary — some render HTML, some don't.
            // MimeKit's BodyBuilder handles both gracefully.
            var body = new BodyBuilder
            {
                // Plain text fallback for basic email clients
                TextBody = $@"Hi {fullName},

                You requested a password reset for your DIFC account.

                Click the link below to reset your password (valid for 15 minutes):
                {resetLink}

                If you did not request this, please ignore this email.
                Your password will not change.

                — DIFC Team",

                // HTML version for modern email clients
                HtmlBody = $@"
                <!DOCTYPE html>
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                  <div style='background: #1565C0; padding: 20px; text-align: center;'>
                    <h2 style='color: white; margin: 0;'>DIFC</h2>
                  </div>
                  <div style='padding: 30px; background: #f9f9f9;'>
                    <h3>Password Reset Request</h3>
                    <p>Hi <strong>{fullName}</strong>,</p>
                    <p>You requested a password reset. Click the button below to set a new password.</p>
                    <p>This link is valid for <strong>15 minutes</strong>.</p>
                    <div style='text-align: center; margin: 30px 0;'>
                      <a href='{resetLink}'
                         style='background: #1565C0; color: white; padding: 14px 28px;
                                text-decoration: none; border-radius: 6px; font-size: 16px;'>
                        Reset Password
                      </a>
                    </div>
                    <p style='color: #888; font-size: 13px;'>
                      If you didn't request this, ignore this email — your password won't change.
                    </p>
                    <p style='color: #888; font-size: 12px; word-break: break-all;'>
                      Or copy this link: {resetLink}
                    </p>
                  </div>
                </body>
                </html>"
            };

            message.Body = body.ToMessageBody();

            // ── Send via SMTP ────────────────────────────────────────────
            using var client = new SmtpClient();

            await client.ConnectAsync(
                settings["Host"],
                int.Parse(settings["Port"]!),
                SecureSocketOptions.StartTls); // STARTTLS on port 587

            await client.AuthenticateAsync(
                settings["Username"],
                settings["Password"]);

            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);

            _logger.LogInformation(
                "Password reset email sent to {Email}", toEmail);
        }
    }
}

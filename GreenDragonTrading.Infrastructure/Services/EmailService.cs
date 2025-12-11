using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace GreenDragonTrading.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailOptions> emailOptions, ILogger<EmailService> logger)
        {
            _emailOptions = emailOptions.Value;
            _logger = logger;
        }

        public async Task SendVerificationEmailAsync(string toEmail, string verificationToken, string verificationUrl, CancellationToken cancellationToken = default)
        {
            var subject = "Verify Your Email - Green Dragon Trading";
            var verifyLink = $"{verificationUrl}?token={Uri.EscapeDataString(verificationToken)}";

            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c5f2d;'>Email Verification</h2>
                        <p>Thank you for registering with Green Dragon Trading.</p>
                        <p>Please click the button below to verify your email address:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{verifyLink}' 
                               style='background-color: #2c5f2d; 
                                      color: white; 
                                      padding: 12px 30px; 
                                      text-decoration: none; 
                                      border-radius: 5px; 
                                      display: inline-block;'>
                                Verify Email
                            </a>
                        </div>
                        <p style='color: #666; font-size: 14px;'>Or copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #666; font-size: 12px;'>{verifyLink}</p>
                        <p style='color: #999; font-size: 12px; margin-top: 30px;'>
                            This link will expire in 1 hours. If you didn't request this, please ignore this email.
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body, cancellationToken);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
        {
            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(_emailOptions.SenderEmail, _emailOptions.SenderName);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using var client = new SmtpClient(_emailOptions.SmtpHost, _emailOptions.SmtpPort);
                client.Credentials = new NetworkCredential(_emailOptions.Username, _emailOptions.Password);
                client.EnableSsl = _emailOptions.EnableSsl;

                await client.SendMailAsync(message, cancellationToken);
                _logger.LogInformation("Gửi email thành công {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gửi email không thành công {Email}", toEmail);
                throw;
            }
        }
    }
}

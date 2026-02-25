namespace GreenDragonTrading.Infrastructure.Services
{
    using GreenDragonTrading.Application.Common.Options;
    using GreenDragonTrading.Application.Interfaces;
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using MimeKit;

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

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
        {
            var subject = "Reset Your Password - Green Dragon Trading";

            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c5f2d;'>Password Reset Request</h2>
                        <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                        <p>Sử dụng mã OTP bên dưới để đặt lại mật khẩu:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <div style='background-color: #f5f5f5; 
                                        padding: 20px; 
                                        border-radius: 5px; 
                                        display: inline-block;'>
                                <h1 style='margin: 0; color: #2c5f2d; letter-spacing: 5px;'>{resetToken}</h1>
                            </div>
                        </div>
                        <p style='color: #666;'>Mã này sẽ hết hạn sau <strong>15 phút</strong>.</p>
                        <p style='color: #999; font-size: 12px; margin-top: 30px;'>
                            Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.
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
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_emailOptions.SenderName, _emailOptions.SenderEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = body };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                _logger.LogInformation("Connecting to SMTP {Host}:{Port}", _emailOptions.SmtpHost, _emailOptions.SmtpPort);

                await smtp.ConnectAsync(
                    _emailOptions.SmtpHost,
                    _emailOptions.SmtpPort,
                    SecureSocketOptions.StartTls,
                    timeoutCts.Token);

                _logger.LogInformation("Authenticating with username: {Username}", _emailOptions.Username);

                await smtp.AuthenticateAsync(
                    _emailOptions.Username,
                    _emailOptions.Password,
                    timeoutCts.Token);

                await smtp.SendAsync(email, timeoutCts.Token);
                await smtp.DisconnectAsync(true, timeoutCts.Token);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }
    }
}

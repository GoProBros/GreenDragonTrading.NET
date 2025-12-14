using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenDragonTrading.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string toEmail, string verificationToken, string verificationUrl, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Send password reset email with OTP token
        /// </summary>
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default);
    }
}

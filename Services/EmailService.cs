using System.Net;
using System.Net.Mail;

namespace Splitkaro.API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtp(string toEmail, string otp)
        {
            var smtp = new SmtpClient(_config["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_config["EmailSettings:Port"]!),
                Credentials = new NetworkCredential(
                    _config["EmailSettings:Username"],
                    _config["EmailSettings:Password"]
                ),
                EnableSsl = true
            };

            var mail = new MailMessage(
                _config["EmailSettings:FromEmail"]!,
                toEmail,
                "Your OTP",
                $"Your OTP is {otp}. It is valid for 5 minutes."
            );

            await smtp.SendMailAsync(mail);
        }
    }
}
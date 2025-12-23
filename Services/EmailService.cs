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

        public async Task SendOtp(string email, string otp)
        {
            var client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(
                    _config["EmailSettings:Username"],
                    _config["EmailSettings:Password"]
                ),
                EnableSsl = true
            };

            var mail = new MailMessage(
                _config["EmailSettings:FromEmail"],
                email,
                "Splitkaro OTP",
                $"Your OTP is {otp}. Valid for 5 minutes."
            );

            await client.SendMailAsync(mail);
        }
    }
}

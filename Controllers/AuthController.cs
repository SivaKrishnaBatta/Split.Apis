using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Splitkaro.API.Data;
using Splitkaro.API.Models;
using Splitkaro.API.Services;

namespace Splitkaro.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public AuthController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            var otp = new Random().Next(100000, 999999).ToString();

            var record = new EmailOtp
            {
                Email = email,
                Otp = otp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            _context.EmailOtps.Add(record);
            await _context.SaveChangesAsync();

            await _emailService.SendOtp(email, otp);

            return Ok("OTP sent");
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var otp = await _context.EmailOtps
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(x =>
                    x.Email == request.Email &&
                    x.Otp == request.Otp &&
                    !x.IsUsed);

            if (otp == null || otp.ExpiresAt < DateTime.UtcNow)
                return BadRequest("Invalid or expired OTP");

            otp.IsUsed = true;
            await _context.SaveChangesAsync();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            return Ok(new { isNewUser = user == null });
        }
    }

    public class VerifyOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
}
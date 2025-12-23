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

        // ================= SEND OTP =================
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required");

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

            return Ok("OTP sent (valid for 5 minutes)");
        }

        // ================= RESEND OTP =================
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required");

            // 1️⃣ Mark all previous OTPs as used
            var oldOtps = await _context.EmailOtps
                .Where(x => x.Email == email && !x.IsUsed)
                .ToListAsync();

            foreach (var otp in oldOtps)
            {
                otp.IsUsed = true;
            }

            // 2️⃣ Generate new OTP
            var newOtp = new Random().Next(100000, 999999).ToString();

            var record = new EmailOtp
            {
                Email = email,
                Otp = newOtp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            _context.EmailOtps.Add(record);
            await _context.SaveChangesAsync();

            // 3️⃣ Send email
            await _emailService.SendOtp(email, newOtp);

            return Ok("OTP resent successfully (valid for 5 minutes)");
        }

        // ================= VERIFY OTP =================
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var data = await _context.EmailOtps
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(x =>
                    x.Email == request.Email &&
                    x.Otp == request.Otp &&
                    !x.IsUsed);

            if (data == null)
                return BadRequest("Invalid OTP");

            if (data.ExpiresAt < DateTime.UtcNow)
                return BadRequest("OTP expired");

            // Mark OTP as used
            data.IsUsed = true;
            await _context.SaveChangesAsync();

            // Check if user exists
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            return Ok(new
            {
                isNewUser = user == null
            });
        }

        // ================= COMPLETE PROFILE =================
        [HttpPost("complete-profile")]
        public async Task<IActionResult> CompleteProfile([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest("Email is required");

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Profile saved successfully");
        }
    }

    // ================= REQUEST DTO =================
    public class VerifyOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
}

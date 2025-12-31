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

        // ✅ SEND OTP
[HttpPost("send-otp")]
public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
{
    var otp = new Random().Next(100000, 999999).ToString();

    var record = new EmailOtp
    {
        Email = request.Email,
        Otp = otp,
        ExpiresAt = DateTime.UtcNow.AddMinutes(5),
        IsUsed = false
    };

    _context.EmailOtps.Add(record);
    await _context.SaveChangesAsync();

    await _emailService.SendOtp(request.Email, otp);

    return Ok(new { message = "OTP sent" });

}


        // ✅ VERIFY OTP
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

            return Ok(new { verified = true });
        }

        // ✅ REGISTER (EMAIL MUST BE OTP VERIFIED)
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    // 1️⃣ Password validation
    if (request.Password != request.ConfirmPassword)
        return BadRequest("Passwords do not match");

    // 2️⃣ Ensure email OTP already verified
    var otpVerified = await _context.EmailOtps
        .AnyAsync(x => x.Email == request.Email && x.IsUsed);

    if (!otpVerified)
        return BadRequest("Email not verified via OTP");

    // 3️⃣ Prevent duplicate registration
    var userExists = await _context.Users
        .AnyAsync(x => x.Email == request.Email);

    if (userExists)
        return BadRequest("Email already registered");

    // 4️⃣ Create user
    var user = new User
    {
        Name = request.Name,
        Email = request.Email,   // 🔒 locked to OTP email
        Phone = request.Phone,
       PasswordHash = PasswordService.HashPassword(request.Password),
        CreatedAt = DateTime.UtcNow
    };

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Registration successful" });
}

    }

    // ✅ DTOs
    public class SendOtpRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
    
}
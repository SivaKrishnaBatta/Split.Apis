using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Splitkaro.API.Models
{
    [Table("email_otps")]
    public class EmailOtp
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("otp")]
        public string Otp { get; set; } = string.Empty;

        [Column("expires_at")]          // ✅ FIX
        public DateTime ExpiresAt { get; set; }

        [Column("is_used")]             // ✅ FIX
        public bool IsUsed { get; set; }
    }
}
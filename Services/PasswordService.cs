using System.Security.Cryptography;
using System.Text;

namespace Splitkaro.API.Services
{
    public static class PasswordService
    {
        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(
                sha.ComputeHash(Encoding.UTF8.GetBytes(password))
            );
        }
    }
}
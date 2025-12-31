using Microsoft.EntityFrameworkCore;
using Splitkaro.API.Models;

namespace Splitkaro.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<EmailOtp> EmailOtps { get; set; }
    }
}

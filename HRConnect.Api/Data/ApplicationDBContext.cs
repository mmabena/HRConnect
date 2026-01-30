namespace HRConnect.Api.Data
{
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions) 
        : base(dbContextOptions)
        {
            
        }
        public DbSet<User> Users { get; set; }
        public DbSet<PasswordResetPin> PasswordResetPins { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }
         
    }
}
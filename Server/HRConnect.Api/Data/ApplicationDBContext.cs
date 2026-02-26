namespace HRConnect.Api.Data
{
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  public class ApplicationDBContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
  {
    public DbSet<User> Users { get; set; }
    public DbSet<PasswordResetPin> PasswordResetPins { get; set; }
    public DbSet<PasswordHistory> PasswordHistories { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<PensionFund> PensionFunds { get; set; }
    public DbSet<PensionOption> PensionOptions { get; set; }

    public DbSet<Position> Positions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      //Employee to pension option
      _ = modelBuilder.Entity<Employee>()
        .HasOne(e => e.PensionFund)
        .WithMany(pf => pf.Employees)
        .HasForeignKey(e => e.PensionFundId)
        .OnDelete(DeleteBehavior.Restrict);

      _ = modelBuilder.Entity<Employee>()
        .HasOne(e => e.PensionOption)
        .WithMany(po => po.Employees)
        .HasForeignKey(e => e.PensionOptionId)
        .OnDelete(DeleteBehavior.Restrict);

    }

  }
}
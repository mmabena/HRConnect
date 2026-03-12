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

      // PensionFund -> Employee relationship
     _ = modelBuilder.Entity<PensionFund>()
          .HasOne(pf => pf.Employee)
          .WithMany(e => e.PensionFunds)
          .HasForeignKey(pf => pf.EmployeeId)
          .OnDelete(DeleteBehavior.Restrict);

      // Employee -> PensionOption relationship
     _ = modelBuilder.Entity<Employee>()
          .HasOne(e => e.PensionOption)
          .WithMany(po => po.Employees)
          .HasForeignKey(e => e.PensionOptionId)
          .OnDelete(DeleteBehavior.Restrict);
    }

  }
}
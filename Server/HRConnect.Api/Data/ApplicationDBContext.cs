namespace HRConnect.Api.Data
{
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;
  using System;


  public class ApplicationDBContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
  {
    public DbSet<User> Users { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<JobGrade> JobGrades { get; set; }
    public DbSet<OccupationalLevel> OccupationalLevels { get; set; }
    public DbSet<PasswordResetPin> PasswordResetPins { get; set; }
    public DbSet<PasswordHistory> PasswordHistories { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
      // Position - JobGrade
      _ = modelBuilder.Entity<Position>()
      .HasOne(p => p.JobGrade)
      .WithMany(jg=> jg.Positions)
      .HasForeignKey(p => p.JobGradeId);

      // Position - OccupationalLevel
      _ = modelBuilder.Entity<Position>()
      .HasOne(p => p.OccupationalLevels)
      .WithMany(o => o.Positions)
      .HasForeignKey(p => p.OccupationalLevelId);

      _ =modelBuilder.Entity<Position>()
      .HasIndex(p => p.Title)
      .IsUnique();
    }
  }
}
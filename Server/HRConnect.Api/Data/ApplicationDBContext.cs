namespace HRConnect.Api.Data
{
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
        : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<PasswordResetPin> PasswordResetPins { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }

        public DbSet<PayrollDeduction> PayrollDeductions { get; set; }
        public DbSet<TaxTableUpload> TaxTableUploads { get; set; }
        public DbSet<TaxDeduction> TaxDeductions { get; set; }
        public DbSet<AuditPayrollDeductions> AuditPayrollDeductions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee relationships
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.PositionId);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.CareerManager)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.CareerManagerID)
                .OnDelete(DeleteBehavior.Restrict);

            // Enum conversions
            modelBuilder.Entity<Employee>().Property(e => e.Title).HasConversion<string>();
            modelBuilder.Entity<Employee>().Property(e => e.Gender).HasConversion<string>();
            modelBuilder.Entity<Employee>().Property(e => e.Branch).HasConversion<string>();
            modelBuilder.Entity<Employee>().Property(e => e.EmploymentStatus).HasConversion<string>();

            // TaxDeduction
            modelBuilder.Entity<TaxDeduction>(entity =>
            {
                entity.ToTable("TaxDeduction");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TaxYear).IsRequired();
                entity.Property(e => e.Remuneration).HasPrecision(12, 2).IsRequired();
                entity.Property(e => e.AnnualEquivalent).HasPrecision(12, 2).IsRequired();
                entity.Property(e => e.TaxUnder65).HasPrecision(12, 2).IsRequired();
                entity.Property(e => e.Tax65To74).HasPrecision(12, 2).IsRequired();
                entity.Property(e => e.TaxOver75).HasPrecision(12, 2).IsRequired();
                entity.HasIndex(e => new { e.TaxYear, e.Remuneration }).IsUnique();
            });

            // TaxTableUpload
            modelBuilder.Entity<TaxTableUpload>(entity =>
            {
                entity.ToTable("TaxTableUpload");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TaxYear).IsRequired();
                entity.Property(e => e.FileName).IsRequired();
                entity.Property(e => e.FileUrl).IsRequired();
                entity.Property(e => e.EffectiveFrom).IsRequired();
            });
        }
    }
}
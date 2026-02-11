namespace HRConnect.Api.Data
{
  using System.Reflection.Metadata;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  public class ApplicationDBContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
  {
    public DbSet<User> Users { get; set; }
    public DbSet<PasswordResetPin> PasswordResetPins { get; set; }
    public DbSet<PasswordHistory> PasswordHistories { get; set; }
    public DbSet<TaxTableUpload> TaxTableUploads { get; set; }
    public DbSet<TaxDeduction> TaxDeductions { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
      modelBuilder.Entity<TaxDeduction>(entity =>
      {
        entity.ToTable("TaxDeduction");
        entity.HasKey(entity => entity.Id);
        entity.Property(entity => entity.TaxYear).IsRequired();
        entity.Property(entity => entity.Remuneration).HasPrecision(12, 2).IsRequired();
        entity.Property(entity => entity.AnnualEquivalent).HasPrecision(12, 2).IsRequired();
        entity.Property(entity => entity.TaxUnder65).HasPrecision(12, 2).IsRequired();
        entity.Property(entity => entity.Tax65To74).HasPrecision(12, 2).IsRequired();
        entity.Property(entity => entity.TaxOver75).HasPrecision(12, 2).IsRequired();
        entity.HasIndex(entity => new { entity.TaxYear, entity.Remuneration }).IsUnique();
      });

      modelBuilder.Entity<TaxTableUpload>(entity =>
      {
        entity.ToTable("TaxTableUpload");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.TaxYear).IsRequired();
        entity.Property(e => e.FileName).IsRequired();
        entity.Property(e => e.FileUrl).IsRequired();
        entity.Property(e => e.UploadedAt);
        entity.Property(e => e.EffectiveFrom).IsRequired();
        entity.Property(e => e.EffectiveTo);
      });

    }
  }
}
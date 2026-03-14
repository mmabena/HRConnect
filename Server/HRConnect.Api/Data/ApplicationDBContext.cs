namespace HRConnect.Api.Data
{
  using System.Reflection;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using Microsoft.EntityFrameworkCore;

  public class ApplicationDBContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
  {
    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<JobGrade> JobGrades { get; set; }
    public DbSet<OccupationalLevel> OccupationalLevels { get; set; }
    public DbSet<PasswordResetPin> PasswordResetPins { get; set; }
    public DbSet<PasswordHistory> PasswordHistories { get; set; }
    public DbSet<MedicalOption> MedicalOptions { get; set; }
    public DbSet<MedicalOptionCategory> MedicalOptionCategories { get; set; }
    public DbSet<TaxTableUpload> TaxTableUploads { get; set; }
    public DbSet<TaxDeduction> TaxDeductions { get; set; }
    public DbSet<StatutoryContribution> StatutoryContributions { get; set; }
    public DbSet<AuditLogs> AuditLogs { get; set; }
    public DbSet<StatutoryContributionType> StatutoryContributionTypes { get; set; }
    public DbSet<PayrollPeriod> PayrollPeriods { get; set; }
    public DbSet<PayrollRun> PayrollRuns { get; set; }
    // public DbSet<PayrollRecord> PayrollRecords { get; set; }
    public DbSet<PensionOption> PensionOptions { get; set; }
    public DbSet<EmployeePensionEnrollment> EmployeePensionEnrollments { get; set; }
    public DbSet<PensionDeduction> PensionDeductions { get; set; }
    public DbSet<MedicalAidDeduction> MedicalAidDeductions { get; set; }
    //public DbSet<LunchDeduction> LunchDeductions { get; set; }
    public DbSet<TestEntity> TestEntities { get; set; }
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

      // Position - OccupationalLevel
      modelBuilder.Entity<Position>()
          .HasOne(p => p.OccupationalLevels)
          .WithMany(o => o.Positions)
          .HasForeignKey(p => p.OccupationalLevelId)
          .OnDelete(DeleteBehavior.Restrict);  // <-- prevent cascade

      // Unique index on Position.PositionTitle
      modelBuilder.Entity<Position>()
          .HasIndex(p => p.PositionTitle)
          .IsUnique();

      // Unique index on OccupationalLevel.Description
      modelBuilder.Entity<OccupationalLevel>()
          .HasIndex(o => o.Description)
          .IsUnique();

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
        entity.Property(e => e.UploadedAt);
        entity.Property(e => e.EffectiveFrom).IsRequired();
        entity.Property(e => e.EffectiveTo);
      });

      // StatutoryContributionType with default contribution percentages mandated by law
      modelBuilder.Entity<StatutoryContributionType>().Property(e => e.EmployeeRate)
        .HasColumnType("decimal(18,4)")
        .HasDefaultValue(0.01m);
      modelBuilder.Entity<StatutoryContributionType>().Property(e => e.EmployerRate)
        .HasColumnType("decimal(18,4)")
        .HasDefaultValue(0.01m);


      /*                                                                       Payroll run*/
      // modelBuilder.Ignore<PayrollRecord>();

      modelBuilder.Entity<PayrollPeriod>().HasMany(p => p.Runs)
      .WithOne(r => r.Period)
      .HasForeignKey(p => p.PeriodId);

      //EF needs to know that PayrollRecord is base type (abstract)
      modelBuilder.Entity<PayrollRecord>().UseTpcMappingStrategy();

      //EF needs to know derived types
      modelBuilder.Entity<PensionDeduction>().ToTable("PensionDeductions");
      modelBuilder.Entity<MedicalAidDeduction>().ToTable("MedicalAidDeductions");


      //Declare PayrollRunId as an alternative Key that can be used instead of Id
      modelBuilder.Entity<PayrollRun>().HasAlternateKey(r => r.PayrollRunId);
      

      //Relationship to payroll run
      modelBuilder.Entity<PayrollRun>().HasMany(p => p.Records)
      .WithOne(r => r.PayrollRun)
      .HasForeignKey(r => r.PayrollRunId)
      .HasPrincipalKey(p => p.PayrollRunId);

      //Testing that any other table can have runID FK
      // modelBuilder.Entity<TestEntity>().HasOne<PayrollRun>()
      // .WithMany()
      // .HasForeignKey(t => t.PayrollRunId);
      modelBuilder.Entity<PayrollRun>(b =>
        {
          b.HasKey(r => r.PayrollRunId);
          b.Property(r => r.PayrollRunId).ValueGeneratedOnAdd();//Identity
          // b.HasCheckConstraint("CK_PayrollRun_PayRunNumber",
          //                "[PayRunNumber] BETWEEN 1 AND 12");

          b.HasMany(r => r.Records)
       .WithOne(r => r.PayrollRun)
       .HasForeignKey(r => r.PayrollRunId);
        });
      // Medical Aid Deduction Delete Nehavior
      modelBuilder.Entity<MedicalAidDeduction>()
        .HasOne(m => m.MedicalOption)
        .WithMany()
        .HasForeignKey(m => m.MedicalOptionId)
        .OnDelete(DeleteBehavior.NoAction);

      modelBuilder.Entity<MedicalAidDeduction>()
        .HasOne(m => m.MedicalOptionCategory)
        .WithMany()
        .HasForeignKey(m => m.MedicalCategoryId)
        .OnDelete(DeleteBehavior.NoAction);

      /*                                                                              Payroll run*/

      /*I am here*/

      modelBuilder.Entity<PensionOption>()
        .HasMany(e => e.Employee)
        .WithOne(po => po.PensionOption)
        .HasForeignKey(po => po.PensionOptionId)
        .OnDelete(DeleteBehavior.SetNull);

      modelBuilder.Entity<Employee>()
        .HasMany(epe => epe.EmployeePensionEnrollment)
        .WithOne(e => e.Employee)
        .HasForeignKey(e => e.EmployeeId)
        .OnDelete(DeleteBehavior.Cascade)
        .IsRequired();

      modelBuilder.Entity<PensionOption>()
        .HasMany(epe => epe.EmployeePensionEnrollment)
        .WithOne(po => po.PensionOption)
        .HasForeignKey(po => po.PensionOptionId)
        .OnDelete(DeleteBehavior.Cascade)
        .IsRequired();


      /*modelBuilder.Entity<PensionDeduction>()
        .HasOne(e => e.Employee)
        .WithMany(pd => pd.PensionDeduction)
        .HasForeignKey(e => e.EmployeeId)
        .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<PensionDeduction>()
        .HasOne(po => po.PensionOption)
        .WithMany(pd => pd.PensionDeduction)
        .HasForeignKey(po => po.PensionOptionId)
        .OnDelete(DeleteBehavior.Cascade);*/

      /*modelBuilder.Entity<PayrollRun>()
        .HasMany(epe => epe.EmployeePensionEnrollment)
        .WithOne(pr => pr.PayrollRun)
        .HasForeignKey(pr => pr.PayrollRunId)
        .IsRequired();

      modelBuilder.Entity<PayrollRun>()
        .HasMany(pd => pd.PensionDeduction)
        .WithOne(pr => pr.PayrollRun)
        .HasForeignKey(pr => pr.PayrollRunId)
        .IsRequired();*/

      /*modelBuilder.Entity<PayrollRecord>()
        .UseTpcMappingStrategy();

      /*modelBuilder.Entity<PayrollRecord>()
        .ToTable("PayrollRecords");

      modelBuilder.Entity<PensionDeduction>()
        .ToTable("PensionDeductions")
        .HasKey(pd => pd.PensionDeductionId);

      modelBuilder.Entity<LunchDeduction>()
        .ToTable("LunchDeductions")
        .HasKey(ld => ld.LunchDeductionId);*/
      modelBuilder.Entity<EmployeePensionEnrollment>().HasOne<PayrollRun>()
      .WithMany()
      .HasForeignKey(t => t.PayrollRunId)
      .HasPrincipalKey(p => p.PayrollRunId);
    }
    //Override 'SaveChangesAsync' for Payroll Records to enforce locked records on a payroll run 
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      //Intercept all instances of saving any changes to db
      var modifiedRecords = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified &&
            (
            e.Entity is PayrollPeriod ||
            e.Entity is PayrollRun ||
            e.Entity is PayrollRecord
            ));

      foreach (var e in modifiedRecords)
      {
        //Any locked entity should be under a Hard Lock. Don't allow any changes
        var prevLockState = (bool)e.OriginalValues["IsLocked"]!;
        if (prevLockState)
        {
          throw new InvalidOperationException("Record/Run under Hard Lock. Cannot be modified");
        }
      }

      // //Do the same locking for entities to prevent deletion
      // modifiedRecords = ChangeTracker.Entries()
      //      .Where(e => e.State == EntityState.Deleted &&
      //      (
      //      e.Entity is PayrollPeriod ||
      //      e.Entity is PayrollRun ||
      //      e.Entity is PayrollRecord
      //      ));

      // foreach (var e in modifiedRecords)
      // {
      //   //Any locked entity should be under a Hard Lock. Don't allow any changes
      //   var prevLockState = (bool)e.OriginalValues["IsLocked"]!;
      //   if (prevLockState)
      //   {
      //     throw new InvalidOperationException("Record/Run under Hard Lock. Cannot be modified");
      //   }
      // }
      return await base.SaveChangesAsync(cancellationToken);
    }
  }
}
namespace HRConnect.Api.Data
{
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using Microsoft.EntityFrameworkCore;
  using AppAny.Quartz.EntityFrameworkCore.Migrations;
  using AppAny.Quartz.EntityFrameworkCore.Migrations.SqlServer;

  public class ApplicationDBContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
  {
    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<JobGrade> JobGrades { get; set; }
    public DbSet<OccupationalLevel> OccupationalLevels { get; set; }
    public DbSet<PasswordResetPin> PasswordResetPins { get; set; }
    public DbSet<PasswordHistory> PasswordHistories { get; set; }

    // Payroll (MAIN)
    public DbSet<MedicalOption> MedicalOptions { get; set; }
    public DbSet<MedicalOptionCategory> MedicalOptionCategories { get; set; }
    public DbSet<TaxTableUpload> TaxTableUploads { get; set; }
    public DbSet<TaxDeduction> TaxDeductions { get; set; }
    public DbSet<StatutoryContribution> StatutoryContributions { get; set; }
    public DbSet<AuditLogs> AuditLogs { get; set; }
    public DbSet<StatutoryContributionType> StatutoryContributionTypes { get; set; }
    public DbSet<PayrollPeriod> PayrollPeriods { get; set; }
    public DbSet<PayrollRun> PayrollRuns { get; set; }
    public DbSet<PayrollRecord> PayrollRecords { get; set; }

    // INJECTED (LEAVE SYSTEM)
    public DbSet<LeaveType> LeaveTypes { get; set; }
    public DbSet<LeaveEntitlementRule> LeaveEntitlementRules { get; set; }
    public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances { get; set; }
    public DbSet<LeaveApplication> LeaveApplications { get; set; }
    public DbSet<EmployeeAccrualRateHistory> EmployeeAccrualRateHistories { get; set; }
    public DbSet<AnnualLeaveAccrualHistory> AnnualLeaveAccrualHistories { get; set; }
    public DbSet<PensionOption> PensionOptions { get; set; }
    public DbSet<EmployeePensionEnrollment> EmployeePensionEnrollments { get; set; }
    public DbSet<PensionDeduction> PensionDeductions { get; set; }
    public DbSet<MedicalAidDeduction> MedicalAidDeductions { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
      // Creating namespace for Quartz migrations separate from HRConnect.dbo 
      modelBuilder.AddQuartz(builder =>
      {
        builder.UseSqlServer(schema: "quartz", prefix: "QRTZ_");
      });

      // ================= MAIN CONFIG =================

      modelBuilder.Entity<Employee>()
          .HasOne(e => e.Position)
          .WithMany(p => p.Employees)
          .HasForeignKey(e => e.PositionId);

      modelBuilder.Entity<Employee>()
          .HasOne(e => e.CareerManager)
          .WithMany(e => e.Subordinates)
          .HasForeignKey(e => e.CareerManagerID)
          .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<Position>()
          .HasOne(p => p.OccupationalLevels)
          .WithMany(o => o.Positions)
          .HasForeignKey(p => p.OccupationalLevelId)
          .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<Position>()
          .HasIndex(p => p.PositionTitle)
          .IsUnique();

      modelBuilder.Entity<OccupationalLevel>()
          .HasIndex(o => o.Description)
          .IsUnique();

      modelBuilder.Entity<Employee>().Property(e => e.Title).HasConversion<string>();
      modelBuilder.Entity<Employee>().Property(e => e.Gender).HasConversion<string>();
      modelBuilder.Entity<Employee>().Property(e => e.Branch).HasConversion<string>();
      modelBuilder.Entity<Employee>().Property(e => e.EmploymentStatus).HasConversion<string>();

      modelBuilder.Entity<Employee>()
              .HasMany(e => e.LeaveBalances)
              .WithOne(b => b.Employee)
              .HasForeignKey(b => b.EmployeeId)
              .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<Employee>()
          .HasMany(e => e.LeaveApplications)
          .WithOne(l => l.Employee)
          .HasForeignKey(l => l.EmployeeId)
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<EmployeeLeaveBalance>()
          .HasOne(lb => lb.LeaveType)
          .WithMany()
          .HasForeignKey(lb => lb.LeaveTypeId)
          .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<LeaveEntitlementRule>()
          .HasOne(r => r.JobGrade)
          .WithMany(j => j.LeaveEntitlementRules)
          .HasForeignKey(r => r.JobGradeId)
          .OnDelete(DeleteBehavior.Restrict);

      // INJECTED FIX: Prevent multiple cascade paths
      modelBuilder.Entity<EmployeeAccrualRateHistory>()
          .HasOne(e => e.Employee)
          .WithMany(e => e.AccrualRateHistory)
          .HasForeignKey(e => e.EmployeeId)
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<EmployeeAccrualRateHistory>()
          .HasOne(e => e.Position)
          .WithMany()
          .HasForeignKey(e => e.PositionId)
          .OnDelete(DeleteBehavior.Restrict);

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

      modelBuilder.Entity<PayrollPeriod>().HasMany(p => p.Runs)
      .WithOne(r => r.Period)
      .HasForeignKey(p => p.PeriodId);

      //EF needs to know that PayrollRecord is a base type (abstract)
      modelBuilder.Entity<PayrollRecord>().UseTpcMappingStrategy();

      //EF needs to know derived types
      modelBuilder.Entity<PensionDeduction>().ToTable("PensionDeductions");
      modelBuilder.Entity<MedicalAidDeduction>().ToTable("MedicalAidDeductions");
      modelBuilder.Entity<StatutoryContribution>().ToTable("StatutoryContributions");

      modelBuilder.Entity<PayrollRun>(b =>
        {
          b.HasKey(r => r.PayrollRunId);
          b.Property(r => r.PayrollRunId).ValueGeneratedOnAdd();
          b.HasMany(r => r.Records)
       .WithOne(r => r.PayrollRun)
       .HasForeignKey(r => r.PayrollRunId);
        });

      // Prevent overwrites and possible race conditions
      // Concurrency tokens are used to make sure that the new entry matches the row being referenced
      // exatcly
      modelBuilder.Entity<PayrollRun>().Property(p => p.IsLocked).IsConcurrencyToken();
      modelBuilder.Entity<PayrollPeriod>().Property(p => p.IsLocked).IsConcurrencyToken();
      modelBuilder.Entity<PayrollRecord>().Property(p => p.IsLocked).IsConcurrencyToken();

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

      modelBuilder.Entity<EmployeePensionEnrollment>().HasOne<PayrollRun>()
      .WithMany()
      .HasForeignKey(t => t.PayrollRunId)
      .HasPrincipalKey(p => p.PayrollRunId);

      //Notifaction Configurations
      modelBuilder.Entity<Notification>().Property(n => n.Severity)
          .HasConversion<string>();

      modelBuilder.Entity<Notification>().Property(n => n.Type)
      .HasConversion<string>();

      // ================= SEED DATA (UNCHANGED FROM YOUR SYSTEM) =================

      modelBuilder.Entity<JobGrade>().HasData(
          new JobGrade { JobGradeId = 1, Name = "Unskilled–Middle", CreatedDate = new DateTime(2024, 1, 1), UpdatedDate = new DateTime(2024, 1, 1) },
          new JobGrade { JobGradeId = 2, Name = "Senior Management", CreatedDate = new DateTime(2024, 1, 1), UpdatedDate = new DateTime(2024, 1, 1) },
          new JobGrade { JobGradeId = 3, Name = "Executive Director", CreatedDate = new DateTime(2024, 1, 1), UpdatedDate = new DateTime(2024, 1, 1) }
      );

      modelBuilder.Entity<OccupationalLevel>().HasData(
        new OccupationalLevel
        {
          OccupationalLevelId = 1,
          Description = "Level 1",
          CreatedDate = new DateTime(2024, 1, 1),
          UpdatedDate = new DateTime(2024, 1, 1)
        },
        new OccupationalLevel
        {
          OccupationalLevelId = 2,
          Description = "Level 2",
          CreatedDate = new DateTime(2024, 1, 1),
          UpdatedDate = new DateTime(2024, 1, 1)
        },
        new OccupationalLevel
        {
          OccupationalLevelId = 3,
          Description = "Level 3",
          CreatedDate = new DateTime(2024, 1, 1),
          UpdatedDate = new DateTime(2024, 1, 1)
        }
      );

      modelBuilder.Entity<Position>().HasData(
          new Position { PositionId = 1, PositionTitle = "Unskilled", JobGradeId = 1, OccupationalLevelId = 1, CreatedDate = new DateTime(2024, 1, 1), UpdatedDate = new DateTime(2024, 1, 1) },
          new Position { PositionId = 2, PositionTitle = "Skilled/Semi Skilled", JobGradeId = 1, OccupationalLevelId = 1, CreatedDate = new DateTime(2024, 1, 1), UpdatedDate = new DateTime(2024, 1, 1) },
          new Position { PositionId = 3, PositionTitle = "Junior Management", JobGradeId = 1, OccupationalLevelId = 1, CreatedDate = new DateTime(2024, 1, 1), UpdatedDate = new DateTime(2024, 1, 1) },
          new Position { PositionId = 4, PositionTitle = "Middle Management", JobGradeId = 1, OccupationalLevelId = 1, CreatedDate = new DateTime(2024, 1, 1), UpdatedDate = new DateTime(2024, 1, 1) },
          new Position { PositionId = 5, PositionTitle = "Top/Senior Management", JobGradeId = 2, OccupationalLevelId = 2, CreatedDate = new DateTime(2024, 1, 1), UpdatedDate = new DateTime(2024, 1, 1) },
          new Position { PositionId = 6, PositionTitle = "Executive Director", JobGradeId = 3, OccupationalLevelId = 3, CreatedDate = new DateTime(2024, 1, 1), UpdatedDate = new DateTime(2024, 1, 1) }
      );

      // Leave Types (Policy stored here)
      modelBuilder.Entity<LeaveType>().HasData(
          new LeaveType
          {
            Id = 1,
            Name = "Annual Leave",
            Code = "AL",
            Description = "Annual Leave Policy",
            ResetMonth = 1,
            ResetDay = 1,
            MaxCarryoverDays = 5,
            CarryoverExpiryMonth = 1,
            CarryoverExpiryDay = 1,
            CarryoverNotificationMonth = 12,
            CarryoverNotificationDay = 1,
            IsRollingWindow = false,
            RollingMonths = null,
            FemaleOnly = false,
            IsActive = true
          },
          new LeaveType
          {
            Id = 2,
            Name = "Sick Leave",
            Code = "SL",
            Description = "Sick Leave Policy",
            ResetMonth = null,
            ResetDay = null,
            IsRollingWindow = true,
            RollingMonths = 36,
            FemaleOnly = false,
            IsActive = true
          },
          new LeaveType
          {
            Id = 3,
            Name = "Maternity Leave",
            Code = "ML",
            Description = "Maternity Leave Policy",
            FemaleOnly = true,
            IsRollingWindow = false,
            IsActive = true
          },
          new LeaveType
          {
            Id = 4,
            Name = "Family Responsibility Leave",
            Code = "FRL",
            Description = "Family Responsibility Policy",
            IsRollingWindow = true,
            RollingMonths = 12,
            FemaleOnly = false,
            IsActive = true
          }
      );

      // Leave Entitlement Rules (ONLY entitlement tiers)
      modelBuilder.Entity<LeaveEntitlementRule>().HasData(
          // Annual <3
          new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 15, IsActive = true },
          new LeaveEntitlementRule { Id = 2, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 18, IsActive = true },
          new LeaveEntitlementRule { Id = 3, LeaveTypeId = 1, JobGradeId = 3, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 22, IsActive = true },

          // Annual 3–5
          new LeaveEntitlementRule { Id = 4, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 18, IsActive = true },
          new LeaveEntitlementRule { Id = 5, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 21, IsActive = true },
          new LeaveEntitlementRule { Id = 6, LeaveTypeId = 1, JobGradeId = 3, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 25, IsActive = true },

          // Annual >5
          new LeaveEntitlementRule { Id = 7, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 20, IsActive = true },
          new LeaveEntitlementRule { Id = 8, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 23, IsActive = true },
          new LeaveEntitlementRule { Id = 9, LeaveTypeId = 1, JobGradeId = 3, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 27, IsActive = true },

          // Sick (all grades)
          new LeaveEntitlementRule { Id = 10, LeaveTypeId = 2, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 30, IsActive = true },
          new LeaveEntitlementRule { Id = 11, LeaveTypeId = 2, JobGradeId = 2, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 30, IsActive = true },
          new LeaveEntitlementRule { Id = 12, LeaveTypeId = 2, JobGradeId = 3, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 30, IsActive = true },

          // Maternity
          new LeaveEntitlementRule { Id = 13, LeaveTypeId = 3, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 120, IsActive = true },
          new LeaveEntitlementRule { Id = 14, LeaveTypeId = 3, JobGradeId = 2, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 120, IsActive = true },
          new LeaveEntitlementRule { Id = 15, LeaveTypeId = 3, JobGradeId = 3, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 120, IsActive = true },

          // Family Responsibility(all grades)
          new LeaveEntitlementRule { Id = 16, LeaveTypeId = 4, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 3, IsActive = true },
          new LeaveEntitlementRule { Id = 17, LeaveTypeId = 4, JobGradeId = 2, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 3, IsActive = true },
          new LeaveEntitlementRule { Id = 18, LeaveTypeId = 4, JobGradeId = 3, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 3, IsActive = true }
      );
    }

    //Override 'SaveChangesAsync' for Payroll Records to enforce locked records on a payroll run 
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      //Intercept all instances of saving any changes to db
      var modifiedRecords = ChangeTracker.Entries()
            .Where(e => (e.State == EntityState.Modified || e.State == EntityState.Deleted) &&
            (
            e.Entity is PayrollPeriod ||
            e.Entity is PayrollRun ||
            e.Entity is PayrollRecord ||
            e.Entity is EmployeePensionEnrollment
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

      return await base.SaveChangesAsync(cancellationToken);
    }
  }
}
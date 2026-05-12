namespace HRConnect.Api.Data
{
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.CompanyContributions;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Models.Payroll.Earning;
  using Microsoft.EntityFrameworkCore;
  using AppAny.Quartz.EntityFrameworkCore.Migrations;
  using AppAny.Quartz.EntityFrameworkCore.Migrations.SqlServer;

  public class ApplicationDBContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
  {
    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<BankingDetail> BankingDetails { get; set; }
    public DbSet<BankBranchCode> BankBranchCodes { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<JobGrade> JobGrades { get; set; }
    public DbSet<OccupationalLevel> OccupationalLevels { get; set; }
    public DbSet<PasswordResetPin> PasswordResetPins { get; set; }
    public DbSet<PasswordHistory> PasswordHistories { get; set; }

    // Payroll
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
    public DbSet<PensionFund> PensionFunds { get; set; }
    public DbSet<PayrollEarning> PayrollEarnings { get; set; }

    // Company Contributions
    public DbSet<CompanyContribution> CompanyContributions { get; set; }
    public DbSet<EmployeeCompanyContribution> EmployeeCompanyContributions { get; set; }

    // Leave System
    public DbSet<LeaveType> LeaveTypes { get; set; }
    public DbSet<LeaveEntitlementRule> LeaveEntitlementRules { get; set; }
    public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances { get; set; }
    public DbSet<LeaveApplication> LeaveApplications { get; set; }
    public DbSet<EmployeeAccrualRateHistory> EmployeeAccrualRateHistories { get; set; }
    public DbSet<AnnualLeaveAccrualHistory> AnnualLeaveAccrualHistories { get; set; }
    public DbSet<JobGradeGroupMap> JobGradeGroupMaps { get; set; }

    public DbSet<PensionOption> PensionOptions { get; set; }
    public DbSet<EmployeePensionEnrollment> EmployeePensionEnrollments { get; set; }
    public DbSet<PensionDeduction> PensionDeductions { get; set; }
    public DbSet<MedicalAidDeduction> MedicalAidDeductions { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.AddQuartz(builder =>
      {
        builder.UseSqlServer(schema: "quartz", prefix: "QRTZ_");
      });

      // Employee Relationships
      modelBuilder.Entity<Employee>()
          .HasOne(e => e.Position)
          .WithMany(p => p.Employees)
          .HasForeignKey(e => e.PositionId);

      modelBuilder.Entity<Employee>()
          .HasOne(e => e.CareerManager)
          .WithMany(e => e.Subordinates)
          .HasForeignKey(e => e.CareerManagerID)
          .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<Employee>()
          .HasOne(e => e.PensionOption)
          .WithMany(po => po.Employees)
          .HasForeignKey(e => e.PensionOptionId)
          .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<PensionFund>()
          .HasOne(pf => pf.Employee)
          .WithMany(e => e.PensionFunds)
          .HasForeignKey(pf => pf.EmployeeId)
          .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<Employee>()
          .HasOne(e => e.BankingDetail)
          .WithOne(b => b.Employee)
          .HasForeignKey<BankingDetail>(b => b.EmployeeId)
          .OnDelete(DeleteBehavior.Cascade);




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
      modelBuilder.Entity<BankingDetail>().Property(b => b.BankName).HasConversion<string>();
      modelBuilder.Entity<BankingDetail>().Property(b => b.AccountType).HasConversion<string>();

      // Leave relationships
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

      modelBuilder.Entity<JobGradeGroupMap>()
          .HasOne(x => x.JobGrade)
          .WithMany()
          .HasForeignKey(x => x.JobGradeId)
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<JobGradeGroupMap>()
          .HasIndex(x => new { x.JobGradeId, x.GroupKey })
          .IsUnique();

      modelBuilder.Entity<EmployeeCompanyContribution>()
    .HasIndex(e => new { e.PayrollRunId, e.EmployeeId })
    .IsUnique();

      // Company Contributions
      modelBuilder.Entity<CompanyContribution>()
          .Property(c => c.Percentage)
          .HasColumnType("decimal(10,6)");

      modelBuilder.Entity<EmployeeCompanyContribution>()
          .Property(e => e.DeathPercentage)
          .HasColumnType("decimal(10,6)");

      modelBuilder.Entity<EmployeeCompanyContribution>()
          .Property(e => e.DisabilityPercentage)
          .HasColumnType("decimal(10,6)");

      modelBuilder.Entity<EmployeeCompanyContribution>()
          .Property(e => e.DeathAmount)
          .HasColumnType("decimal(18,2)");

      modelBuilder.Entity<EmployeeCompanyContribution>()
          .Property(e => e.DisabilityAmount)
          .HasColumnType("decimal(18,2)");

      // Accrual history
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

      // Tax
      modelBuilder.Entity<TaxDeduction>(entity =>
      {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Remuneration).HasPrecision(12, 2);
      });

      modelBuilder.Entity<TaxTableUpload>(entity =>
      {
        entity.HasKey(e => e.Id);
      });

      // Payroll
      modelBuilder.Entity<PayrollRecord>().UseTpcMappingStrategy();

      modelBuilder.Entity<PayrollRun>(b =>
      {
        b.HasKey(r => r.PayrollRunId);
        b.HasMany(r => r.Records)
          .WithOne(r => r.PayrollRun)
          .HasForeignKey(r => r.PayrollRunId);
      });

      modelBuilder.Entity<PayrollRun>().Property(p => p.IsLocked).IsConcurrencyToken();
      modelBuilder.Entity<PayrollPeriod>().Property(p => p.IsLocked).IsConcurrencyToken();
      modelBuilder.Entity<PayrollRecord>().Property(p => p.IsLocked).IsConcurrencyToken();

      // Medical Aid //EmployeeCompanyContributions
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

      // Pension Enrollment
      modelBuilder.Entity<Employee>()
          .HasMany(epe => epe.EmployeePensionEnrollment)
          .WithOne(e => e.Employee)
          .HasForeignKey(e => e.EmployeeId)
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<PensionOption>()
          .HasMany(epe => epe.EmployeePensionEnrollment)
          .WithOne(po => po.PensionOption)
          .HasForeignKey(po => po.PensionOptionId)
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<BankingDetail>()
          .HasOne(b => b.BankBranchCode)
          .WithMany(bc => bc.BankingDetails)
          .HasForeignKey(b => b.BankBranchCodeId)
          .OnDelete(DeleteBehavior.Restrict);
      
    

      modelBuilder.Entity<BankingDetail>()
    .HasIndex(b => b.AccountNumberSearchHash)
    .IsUnique();

      // Notifications
      modelBuilder.Entity<Notification>().Property(n => n.Severity).HasConversion<string>();
      modelBuilder.Entity<Notification>().Property(n => n.Type).HasConversion<string>();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      var modifiedRecords = ChangeTracker.Entries()
            .Where(e => (e.State == EntityState.Modified || e.State == EntityState.Deleted) &&
            (
            e.Entity is PayrollPeriod ||
            e.Entity is PayrollRun ||
            e.Entity is PayrollRecord ||
            e.Entity is EmployeePensionEnrollment ||
            e.Entity is BankingDetail
            ));

      foreach (var e in modifiedRecords)
      {
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
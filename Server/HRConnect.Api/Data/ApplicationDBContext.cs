namespace HRConnect.Api.Data
{
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils.PositionAndLeaveSeed;
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
    // LEAVE SYSTEM
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


     


      // SEED DATA: (Position, Job Grade, Occupational Level, Leave Types)
      modelBuilder.Entity<JobGrade>().HasData(SeedData.GetJobGrades());
      modelBuilder.Entity<OccupationalLevel>().HasData(SeedData.GetOccupationalLevels());
      modelBuilder.Entity<Position>().HasData(SeedData.GetPositions());
      modelBuilder.Entity<LeaveType>().HasData(SeedData.GetLeaveTypes());
      modelBuilder.Entity<LeaveEntitlementRule>().HasData(SeedData.GetLeaveEntitlementRules());
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
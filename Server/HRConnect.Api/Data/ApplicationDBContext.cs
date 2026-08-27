namespace HRConnect.Api.Data
{
  using AppAny.Quartz.EntityFrameworkCore.Migrations;
  using AppAny.Quartz.EntityFrameworkCore.Migrations.SqlServer;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.CompanyContributions;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.Payroll.Earning;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Models.Payroll.Earning;
  using Microsoft.EntityFrameworkCore;
  using AppAny.Quartz.EntityFrameworkCore.Migrations;
  using AppAny.Quartz.EntityFrameworkCore.Migrations.SqlServer;
  using System.Collections.Generic;
  using Microsoft.EntityFrameworkCore.ChangeTracking;
  using Microsoft.AspNetCore.DataProtection;
  using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
  using HRConnect.Api.Utils;

  public class ApplicationDBContext(DbContextOptions dbContextOptions, IDataProtectionProvider
  provider) : DbContext(dbContextOptions)
  {
    private readonly IDataProtector _protector = provider.CreateProtector("DbEncryptor");
    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<BankingDetail> BankingDetails { get; set; }
    public DbSet<BankBranchCode> BankBranchCodes { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<JobGrade> JobGrades { get; set; }
    public DbSet<UserCompany> UserCompanies { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<OccupationalLevel> OccupationalLevels { get; set; }
    public DbSet<PasswordResetPin> PasswordResetPins { get; set; }
    public DbSet<PasswordHistory> PasswordHistories { get; set; }
    public DbSet<MedicalOption> MedicalOptions { get; set; }
    public DbSet<MedicalAidDependent> MedicalAidDependents { get; set; }
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

    public DbSet<EmployeePayrollEarning> EmployeePayrollEarnings { get; set; }
    public DbSet<Deduction> Deductions { get; set; }
    public DbSet<EmployeeDeduction> EmployeeDeductions { get; set; }
    public DbSet<TOTPState> TOTPStates { get; set; }
    public DbSet<MFAUserSecret> UserSecrets { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.AddQuartz(builder =>
      {
        builder.UseSqlServer(schema: "quartz", prefix: "QRTZ_");
      });

      // Use this to protector to convert data of your choosing (of type string) in the database
      SecretsProtector.Init(provider.CreateProtector("DbEncryptor"));
      var stringEncryptor = new ValueConverter<string, string>(
        x => x == null ? string.Empty : SecretsProtector.Wrap(x),
        x => x == null ? string.Empty : SecretsProtector.UnWrap<string>(x)
        );

      var byteEncryptor = new ValueConverter<byte[], byte[]>(
        x => x == null ? null : SecretsProtector.WrapBytes(x),
        x => x == null ? null : SecretsProtector.UnWrapBytes(x)
        );


      // Using data protect to encrypt notification messages
      modelBuilder.Entity<Notification>()
      .Property(n => n.Message)
      .HasConversion(stringEncryptor);

      modelBuilder.Entity<MFAUserSecret>()
      .Property(m => m.EncryptedUserSecret)
      .HasConversion(byteEncryptor);


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

      modelBuilder.Entity<UserCompany>()
          .HasKey(uc => new { uc.UserId, uc.CompanyId });

            modelBuilder.Entity<UserCompany>()
                .HasOne(uc => uc.User)
                .WithMany()
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserCompany>()
                .HasOne(uc => uc.Company)
                .WithMany()
                .HasForeignKey(uc => uc.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<OccupationalLevel>()
          .HasIndex(o => o.Description)
          .IsUnique();

      modelBuilder.Entity<PayrollRecord>()
      .HasIndex(x => new { x.PayrollRunId, x.EmployeeId })
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

      modelBuilder.Entity<Employee>()
          .HasOne(e => e.Company)
          .WithMany(c => c.Employees)
          .HasForeignKey(e => e.CompanyId)
          .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Company>()
                .HasIndex(c => c.CompanyId)
                .IsUnique();

            modelBuilder.Entity<Company>()
                .HasIndex(c => c.RegistrationNumber)
                .IsUnique();

            modelBuilder.Entity<Company>()
                .HasIndex(c => c.UIFNumber)
                .IsUnique();

            modelBuilder.Entity<Company>()
                .HasIndex(c => c.VATNumber)
                .IsUnique();

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

            modelBuilder.Entity<LeaveDocument>()
                .HasOne(d => d.LeaveApplication)
                .WithMany(l => l.Documents)
                .HasForeignKey(d => d.LeaveApplicationId);

            modelBuilder.Entity<LeaveApplication>()
                .HasIndex(l => l.Status);

            modelBuilder.Entity<LeaveApplication>()
                .HasIndex(l => l.EmployeeId);

            modelBuilder.Entity<LeaveApplication>()
                .HasIndex(l => l.LeaveTypeId);

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

            // Medical Aid Deduction Delete Behavior
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

            // StatutoryContributionType defaults
            modelBuilder.Entity<StatutoryContributionType>().Property(e => e.EmployeeRate)
              .HasColumnType("decimal(18,4)")
              .HasDefaultValue(0.01m);

            modelBuilder.Entity<StatutoryContributionType>().Property(e => e.EmployerRate)
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0.01m);

      // Payroll relationships
      modelBuilder.Entity<PayrollPeriod>().HasMany(p => p.Runs)
      .WithOne(r => r.Period)
      .HasForeignKey(p => p.PeriodId);



            //EF needs to know that PayrollRecord is a base type (abstract)
            modelBuilder.Entity<PayrollRecord>().UseTpcMappingStrategy();

            //EF needs to know derived types
            modelBuilder.Entity<PensionDeduction>().ToTable("PensionDeductions");
            modelBuilder.Entity<MedicalAidDeduction>().ToTable("MedicalAidDeductions");
            modelBuilder.Entity<EmployeeCompanyContribution>().ToTable("EmployeeCompanyContributions");
            modelBuilder.Entity<StatutoryContribution>().ToTable("StatutoryContributions");

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

      // Medical Aid Deduction Delete Behavior
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

      modelBuilder.Entity<MedicalAidDependent>()
          .HasOne(d => d.Employee)
          .WithMany(e => e.MedicalAidDependents)
          .HasForeignKey(d => d.EmployeeId)
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<MedicalAidDependent>()
          .Property(d => d.Gender)
          .HasConversion<string>();

      modelBuilder.Entity<MedicalAidDependent>()
          .Property(d => d.Relationship)
          .HasConversion<string>();


      modelBuilder.Entity<MedicalAidDependent>()
          .HasIndex(d => d.IdNumber)
          .IsUnique();

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

            //Notifaction Configurations
            modelBuilder.Entity<Notification>().Property(n => n.Severity)
                .HasConversion<string>();
            modelBuilder.Entity<Notification>().Property(n => n.Type)
            .HasConversion<string>();

            modelBuilder.Entity<Employee>()
              .HasMany(epre => epre.EmployeePayrollEarning)
              .WithOne(e => e.Employee)
              .HasForeignKey(e => e.EmployeeId)
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            modelBuilder.Entity<PayrollEarning>()
              .HasMany(epre => epre.EmployeePayrollEarning)
              .WithOne(pre => pre.PayrollEarning)
              .HasForeignKey(pre => pre.PayrollEarningId)
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();



            modelBuilder.Entity<EmployeePayrollEarning>()
              .HasOne<PayrollRun>()
              .WithMany()
              .HasForeignKey(epe => epe.PayrollRunId)
              .HasPrincipalKey(p => p.PayrollRunId);

            modelBuilder.Entity<PayrollEarning>().HasData(
                new PayrollEarning
                {
                    PayrollEarningId = "PRE001",
                    ShortDescription = "Basic salary",
                    LongDescription = "Employee monthly salary",
                    Taxable = true,
                    TaxCode = 3601,
                    TaxPercentage = 100m,
                    OvertimeHourMultiplier = null,
                    CanProRata = true,
                    IsOnGoing = true,
                    IsActive = true
                }
              );

            modelBuilder.Entity<Deduction>().Property(d => d.InputType).HasConversion<string>();

            modelBuilder.Entity<EmployeeDeduction>()
              .HasOne<PayrollRun>()
              .WithMany()
              .HasForeignKey(ed => ed.PayrollRunId)
              .HasPrincipalKey(p => p.PayrollRunId);

            modelBuilder.Entity<Deduction>()
              .HasMany(d => d.EmployeeDeduction)
              .WithOne(ed => ed.Deduction)
              .HasForeignKey(ed => ed.DeductionId)
              .OnDelete(DeleteBehavior.NoAction);

      modelBuilder.Entity<Employee>()
        .HasMany(e => e.EmployeeDeduction)
        .WithOne(ed => ed.Employee)
        .HasForeignKey(ed => ed.EmployeeId)
        .OnDelete(DeleteBehavior.NoAction);

      modelBuilder.Entity<User>()
          .Property(u => u.TempRole)
          .HasConversion<string>();

      modelBuilder.Entity<TOTPState>()
      .HasIndex(u => u.UserId);
            modelBuilder.Entity<Employee>()
              .HasMany(e => e.EmployeeDeduction)
              .WithOne(ed => ed.Employee)
              .HasForeignKey(ed => ed.EmployeeId)
              .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<TOTPState>(entity =>
            {
                entity.HasKey(x => x.UserId);

                entity.Property(x => x.UserId)
                      .ValueGeneratedNever();

                entity.HasOne(x => x.User)
                      .WithOne()
                      .HasForeignKey<TOTPState>(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MFAUserSecret>(entity =>
            {
                entity.HasKey(x => x.UserId);

                entity.Property(x => x.UserId)
                      .ValueGeneratedNever();

                entity.HasOne(x => x.User)
                      .WithOne()
                      .HasForeignKey<MFAUserSecret>(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

        }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      //Intercept all instances of saving any changes to db
      IEnumerable<EntityEntry> modifiedRecords = ChangeTracker.Entries()
            .Where(e => (e.State == EntityState.Modified || e.State == EntityState.Deleted) &&
            (
            e.Entity is PayrollPeriod ||
            e.Entity is PayrollRun ||
            e.Entity is PayrollRecord ||
            e.Entity is EmployeePensionEnrollment ||
            e.Entity is BankingDetail ||
            e.Entity is EmployeePayrollEarning ||
            e.Entity is EmployeeDeduction
            ));

      foreach (EntityEntry e in modifiedRecords)
      {
        //Any locked entity should be under a Hard Lock. Don't allow any changes
        bool prevLockState = (bool)e.OriginalValues["IsLocked"]!;
        if (prevLockState)
        {
          throw new InvalidOperationException("Record/Run under Hard Lock. Cannot be modified");
        }
      }
      return await base.SaveChangesAsync(cancellationToken);
    }
  }
}
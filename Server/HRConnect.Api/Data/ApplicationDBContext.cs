namespace HRConnect.Api.Data
{
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<PasswordResetPin> PasswordResetPins { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }

        public DbSet<JobGrade> JobGrades => Set<JobGrade>();
        public DbSet<Position> Positions => Set<Position>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
        public DbSet<LeaveEntitlementRule> LeaveEntitlementRules => Set<LeaveEntitlementRule>();
        public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances => Set<EmployeeLeaveBalance>();
        public DbSet<LeaveApplication> LeaveApplications => Set<LeaveApplication>();
        public DbSet<EmployeeAccrualRateHistory> EmployeeAccrualRateHistories { get; set; }
        public DbSet<AnnualLeaveAccrualHistory> AnnualLeaveAccrualHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureJobGrade(modelBuilder);
            ConfigurePosition(modelBuilder);
            ConfigureEmployee(modelBuilder);
            ConfigureLeaveType(modelBuilder);
            ConfigureLeaveEntitlementRule(modelBuilder);
            ConfigureEmployeeLeaveBalance(modelBuilder);
            ConfigureLeaveApplication(modelBuilder);
            ConfigureEmployeeAccrualRateHistory(modelBuilder);
            ConfigureAnnualLeaveAccrualHistory(modelBuilder);
            ConfigureDecimalPrecision(modelBuilder);

            SeedData(modelBuilder);
        }

        // ================= CONFIGURATION =================
        private static void ConfigureEmployeeAccrualRateHistory(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeAccrualRateHistory>()
                .HasOne(x => x.Employee)
                .WithMany(e => e.AccrualRateHistory)
                .HasForeignKey(x => x.EmployeeId);
            modelBuilder.Entity<EmployeeAccrualRateHistory>()
                .HasIndex(x => new { x.EmployeeId, x.EffectiveFrom })
                .IsUnique();
        }
        private static void ConfigureJobGrade(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JobGrade>()
                .HasKey(j => j.JobGradeId);

            modelBuilder.Entity<JobGrade>()
                .HasIndex(j => j.Name)
                .IsUnique();
        }

        private static void ConfigurePosition(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Position>()
                .HasKey(p => p.PositionId);

            modelBuilder.Entity<Position>()
                .HasOne(p => p.JobGrade)
                .WithMany(j => j.Positions)
                .HasForeignKey(p => p.JobGradeId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureEmployee(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasKey(e => e.EmployeeId);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureLeaveType(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LeaveType>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<LeaveType>()
                .HasIndex(l => l.Code)
                .IsUnique();
            modelBuilder.Entity<LeaveType>()
                .HasIndex(l => l.Name)
                .IsUnique();
        }

        private static void ConfigureLeaveEntitlementRule(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LeaveEntitlementRule>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<LeaveEntitlementRule>()
                .HasOne(r => r.LeaveType)
                .WithMany(l => l.EntitlementRules)
                .HasForeignKey(r => r.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveEntitlementRule>()
                .HasOne(r => r.JobGrade)
                .WithMany(j => j.LeaveEntitlementRules)
                .HasForeignKey(r => r.JobGradeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveEntitlementRule>()
                .HasIndex(r => new { r.LeaveTypeId, r.JobGradeId, r.MinYearsService })
                .IsUnique();
        }

        private static void ConfigureEmployeeLeaveBalance(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeLeaveBalance>()
                .HasKey(b => b.Id);

            modelBuilder.Entity<EmployeeLeaveBalance>()
                .HasOne(b => b.Employee)
                .WithMany(e => e.LeaveBalances)
                .HasForeignKey(b => b.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeLeaveBalance>()
                .HasOne(b => b.LeaveType)
                .WithMany()
                .HasForeignKey(b => b.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeLeaveBalance>()
                .HasIndex(b => new { b.EmployeeId, b.LeaveTypeId })
                .IsUnique();
        }

        private static void ConfigureLeaveApplication(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LeaveApplication>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<LeaveApplication>()
                .HasOne(l => l.Employee)
                .WithMany(e => e.LeaveApplications)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LeaveApplication>()
                .HasOne(l => l.LeaveType)
                .WithMany()
                .HasForeignKey(l => l.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveApplication>()
                .HasIndex(l => new { l.EmployeeId, l.StartDate, l.EndDate });
        }
        private static void ConfigureAnnualLeaveAccrualHistory(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AnnualLeaveAccrualHistory>()
                .HasOne(x => x.Employee)
                .WithMany(e => e.AnnualLeaveAccrualHistories)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        private static void ConfigureDecimalPrecision(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeLeaveBalance>().Property(p => p.AccruedDays).HasPrecision(10, 2);
            modelBuilder.Entity<EmployeeLeaveBalance>().Property(p => p.AvailableDays).HasPrecision(10, 2);
            modelBuilder.Entity<EmployeeLeaveBalance>().Property(p => p.CarryoverDays).HasPrecision(10, 2);
            modelBuilder.Entity<EmployeeLeaveBalance>().Property(p => p.ForfeitedDays).HasPrecision(10, 2);
            modelBuilder.Entity<EmployeeLeaveBalance>().Property(p => p.TakenDays).HasPrecision(10, 2);

            modelBuilder.Entity<EmployeeAccrualRateHistory>().Property(p => p.AnnualEntitlement).HasPrecision(10, 2);

            modelBuilder.Entity<LeaveApplication>().Property(p => p.DaysRequested).HasPrecision(10, 2);

            modelBuilder.Entity<LeaveEntitlementRule>().Property(p => p.DaysAllocated).HasPrecision(10, 2);
            modelBuilder.Entity<LeaveEntitlementRule>().Property(p => p.MinYearsService).HasPrecision(10, 2);
            modelBuilder.Entity<LeaveEntitlementRule>().Property(p => p.MaxYearsService).HasPrecision(10, 2);

            modelBuilder.Entity<AnnualLeaveAccrualHistory>().Property(p => p.Accrued).HasPrecision(10, 2);
            modelBuilder.Entity<AnnualLeaveAccrualHistory>().Property(p => p.OpeningBalance).HasPrecision(10, 2);
            modelBuilder.Entity<AnnualLeaveAccrualHistory>().Property(p => p.ClosingBalance).HasPrecision(10, 2);
            modelBuilder.Entity<AnnualLeaveAccrualHistory>().Property(p => p.Forfeited).HasPrecision(10, 2);
            modelBuilder.Entity<AnnualLeaveAccrualHistory>().Property(p => p.Used).HasPrecision(10, 2);
        }

        // ================= SEED DATA =================

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Job Grades
            modelBuilder.Entity<JobGrade>().HasData(
                new JobGrade { JobGradeId = 1, Name = "Unskilled–Middle" },
                new JobGrade { JobGradeId = 2, Name = "Senior Management" },
                new JobGrade { JobGradeId = 3, Name = "Executive Director" }
            );

            // Positions
            modelBuilder.Entity<Position>().HasData(
                new Position { PositionId = 1, PositionTitle = "Unskilled", JobGradeId = 1 },
                new Position { PositionId = 2, PositionTitle = "Skilled/Semi Skilled", JobGradeId = 1 },
                new Position { PositionId = 3, PositionTitle = "Junior Management", JobGradeId = 1 },
                new Position { PositionId = 4, PositionTitle = "Middle Management", JobGradeId = 1 },
                new Position { PositionId = 5, PositionTitle = "Top/Senior Management", JobGradeId = 2 },
                new Position { PositionId = 6, PositionTitle = "Executive Director", JobGradeId = 3 }
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

                // Family Responsibility(all grades)
                new LeaveEntitlementRule { Id = 14, LeaveTypeId = 4, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 3, IsActive = true },
                 new LeaveEntitlementRule { Id = 15, LeaveTypeId = 4, JobGradeId = 2, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 3, IsActive = true },
                  new LeaveEntitlementRule { Id = 16, LeaveTypeId = 4, JobGradeId = 3, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 3, IsActive = true }
            );
        }
    }
}

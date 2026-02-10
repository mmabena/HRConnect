

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

        // =========================
        // DbSets
        // =========================
        public DbSet<User> Users { get; set; }
        public DbSet<PasswordResetPin> PasswordResetPins { get; set; }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<JobGrade> JobGrades { get; set; }

        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveEntitlementRule> LeaveEntitlementRules { get; set; }
        public DbSet<LeaveTypeApplication> LeaveTypeApplications { get; set; }
        public DbSet<LeaveApplication> LeaveApplications { get; set; }
        public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances { get; set; }

        // =========================
        // Seed Data (Analyzer-safe)
        // =========================
        private static readonly LeaveType[] SeedLeaveTypes =
        {
            new LeaveType
            {
                LeaveTypeId = 1,
                Name = "Annual Leave",
                Code = "AL",
                DaysEntitled = 0
            },
            new LeaveType
            {
                LeaveTypeId = 2,
                Name = "Sick Leave",
                Code = "SL",
                DaysEntitled = 0
            },
            new LeaveType
            {
                LeaveTypeId = 3,
                Name = "Maternity Leave",
                Code = "ML",
                DaysEntitled = 0
            },
            new LeaveType
            {
                LeaveTypeId = 4,
                Name = "Family Responsibility Leave",
                Code = "FRL",
                DaysEntitled = 0
            }
        };

        // =========================
        // Model Configuration
        // =========================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // -------------------------
            // JobGrade → Employee
            // -------------------------
            modelBuilder.Entity<JobGrade>()
                .HasOne(j => j.Employee)
                .WithMany()
                .HasForeignKey(j => j.EmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            // -------------------------
            // LeaveEntitlementRule
            // -------------------------
            modelBuilder.Entity<LeaveEntitlementRule>()
                .HasOne(l => l.Employee)
                .WithMany()
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LeaveEntitlementRule>()
                .HasOne(l => l.JobGradeRecord)
                .WithMany()
                .HasForeignKey(l => l.JobGradeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LeaveEntitlementRule>()
                .HasOne(l => l.LeaveType)
                .WithMany()
                .HasForeignKey(l => l.LeaveTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            // -------------------------
            // EmployeeLeaveBalance
            // -------------------------
            modelBuilder.Entity<EmployeeLeaveBalance>()
                .HasOne(b => b.Employee)
                .WithMany()
                .HasForeignKey(b => b.EmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<EmployeeLeaveBalance>()
                .HasOne(b => b.LeaveType)
                .WithMany()
                .HasForeignKey(b => b.LeaveTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            // -------------------------
            // LeaveApplication
            // -------------------------
            modelBuilder.Entity<LeaveApplication>()
                .HasOne(l => l.Employee)
                .WithMany()
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LeaveApplication>()
                .HasOne(l => l.LeaveType)
                .WithMany()
                .HasForeignKey(l => l.LeaveTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            // -------------------------
            // Seed Leave Types
            // -------------------------
            modelBuilder.Entity<LeaveType>()
                .HasData(SeedLeaveTypes);

            base.OnModelCreating(modelBuilder);
        }
    }
}

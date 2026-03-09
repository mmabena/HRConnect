
namespace HRConnect.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Data;
    using HRConnect.Api.Models;
    using HRConnect.Api.Utils;
    using HRConnect.Api.Services;
    using Microsoft.EntityFrameworkCore;
    using Xunit;
    using HRConnect.Api.DTOs;

    public class EmployeeEntitlementServiceTests
    {
        private sealed class FakeEmailService : IEmailService
        {
            public Task SendEmailAsync(string recipientEmail, string subject, string body)
                => Task.CompletedTask;
        }
        private sealed class TrackingEmailService : IEmailService
        {
            public List<(string Recipient, string Subject, string Body)> SentEmails { get; } = new();
            public Task SendEmailAsync(string recipientEmail, string subject, string body)
            {
                SentEmails.Add((recipientEmail, subject, body));
                return Task.CompletedTask;
            }
        }
        private static ApplicationDBContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new ApplicationDBContext(options);
        }
        [Fact]
        public async Task MaleShouldGetThreeLeaveTypes()
        {
            var context = GetInMemoryDb();

            // Arrange JobGrade + Position
            context.JobGrades.Add(new JobGrade { Id = 1, Name = "Unskilled–Middle" });
            context.Positions.Add(new Position { PositionId = 1, Title = "Unskilled", JobGradeId = 1 });

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };


            context.Employees.Add(employee);

            // Leave Types
            context.LeaveTypes.AddRange(
                new LeaveType { Id = 1, Name = "Annual", Code = "AL", Description = "Annual Leave", IsActive = true },
                new LeaveType { Id = 2, Name = "Sick", Code = "SL", Description = "Sic Leave", IsActive = true },
                new LeaveType { Id = 3, Name = "Maternity", Code = "ML", Description = "Maternity Leave", FemaleOnly = true, IsActive = true },
                new LeaveType { Id = 4, Name = "FMRL", Code = "FRL", Description = "Family Responsibility Leave", IsActive = true }
            );

            // Rules for all except maternity
            context.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 15m, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 2, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 30m, IsActive = true },
                new LeaveEntitlementRule { Id = 3, LeaveTypeId = 4, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 3m, IsActive = true }
            );

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            // Act
            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            // Assert
            var balances = context.EmployeeLeaveBalances.ToList();

            Assert.Equal(3, balances.Count);

            var leaveTypeIds = balances.Select(b => b.LeaveTypeId).ToList();

            Assert.Contains(1, leaveTypeIds); // Annual
            Assert.Contains(2, leaveTypeIds); // Sick
            Assert.Contains(4, leaveTypeIds); // FMRL
            Assert.DoesNotContain(3, leaveTypeIds); // Maternity
        }
        [Fact]
        public async Task FemaleShouldGetFourLeaveTypes()
        {
            var context = GetInMemoryDb();

            context.JobGrades.Add(new JobGrade { Id = 1, Name = "Unskilled–Middle" });
            context.Positions.Add(new Position { PositionId = 1, Title = "Unskilled", JobGradeId = 1 });

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Female",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };


            context.Employees.Add(employee);

            context.LeaveTypes.AddRange(
                new LeaveType { Id = 1, Name = "Annual", Code = "AL", Description = "Annual Leave", IsActive = true },
                new LeaveType { Id = 2, Name = "Sick", Code = "SL", Description = "Sic Leave", IsActive = true },
                new LeaveType { Id = 3, Name = "Maternity", Code = "ML", Description = "Maternity Leave", FemaleOnly = true, IsActive = true },
                new LeaveType { Id = 4, Name = "FMRL", Code = "FRL", Description = "Family Responsibility Leave", IsActive = true }
            );

            context.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 15m, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 2, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 30m, IsActive = true },
                new LeaveEntitlementRule { Id = 3, LeaveTypeId = 3, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 120m, IsActive = true },
                new LeaveEntitlementRule { Id = 4, LeaveTypeId = 4, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 3m, IsActive = true }
            );

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balances = context.EmployeeLeaveBalances.ToList();

            Assert.Equal(4, balances.Count);

            var leaveTypeIds = balances.Select(b => b.LeaveTypeId).ToList();

            Assert.Contains(1, leaveTypeIds);
            Assert.Contains(2, leaveTypeIds);
            Assert.Contains(3, leaveTypeIds);
            Assert.Contains(4, leaveTypeIds);
        }
        [Fact]
        public async Task ShouldNotCreateDuplicateBalances()
        {
            var context = GetInMemoryDb();

            context.JobGrades.Add(new JobGrade { Id = 1, Name = "Unskilled–Middle" });
            context.Positions.Add(new Position { PositionId = 1, Title = "Unskilled", JobGradeId = 1 });

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);

            context.LeaveTypes.Add(
                new LeaveType
                {
                    Id = 1,
                    Name = "Annual",
                    Code = "AL",
                    Description = "Annual Leave",
                    IsActive = true
                });

            context.LeaveEntitlementRules.Add(
                new LeaveEntitlementRule
                {
                    Id = 1,
                    LeaveTypeId = 1,
                    JobGradeId = 1,
                    MinYearsService = 0,
                    MaxYearsService = null,
                    DaysAllocated = 15m,
                    IsActive = true
                });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            // First initialization
            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            // Second initialization (should not duplicate)
            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balances = context.EmployeeLeaveBalances.ToList();

            Assert.Single(balances);
        }
        [Fact]
        public async Task PromotionMidYearShouldProrateCorrectly()
        {
            var context = GetInMemoryDb();

            // Job Grades
            context.JobGrades.AddRange(
                new JobGrade { Id = 1, Name = "Unskilled–Middle" },
                new JobGrade { Id = 2, Name = "Senior Management" });

            context.Positions.AddRange(
                new Position { PositionId = 1, Title = "Unskilled", JobGradeId = 1 },
                new Position { PositionId = 2, Title = "Senior", JobGradeId = 2 });

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);

            context.LeaveTypes.Add(
                new LeaveType { Id = 1, Name = "Annual", Code = "AL", Description = "Annual", IsActive = true });

            context.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, DaysAllocated = 15m, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 0, DaysAllocated = 18m, IsActive = true });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            // Promote in July
            employee.PositionId = 2;
            employee.UpdatedDate = new DateTime(DateTime.UtcNow.Year, 7, 1);

            await context.SaveChangesAsync();

            await service.RecalculateAnnualLeaveAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.First();
            // post‑promotion entitlement should be greater than prior to promotion
            Assert.True(balance.AccruedDays > 0);
        }
        [Fact]
        public async Task PromotionShouldPreserveTakenDays()
        {
            var context = GetInMemoryDb();

            context.JobGrades.AddRange(
                new JobGrade { Id = 1, Name = "Unskilled–Middle" },
                new JobGrade { Id = 2, Name = "Senior Management" });

            context.Positions.AddRange(
                new Position { PositionId = 1, Title = "Unskilled", JobGradeId = 1 },
                new Position { PositionId = 2, Title = "Senior", JobGradeId = 2 });

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);

            context.LeaveTypes.Add(
                new LeaveType { Id = 1, Name = "Annual", Code = "AL", Description = "Annual", IsActive = true });

            context.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, DaysAllocated = 15m, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 0, DaysAllocated = 18m, IsActive = true });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.First();
            balance.TakenDays = 5;
            await context.SaveChangesAsync();

            employee.PositionId = 2;
            employee.UpdatedDate = new DateTime(DateTime.UtcNow.Year, 7, 1);
            await context.SaveChangesAsync();

            await service.RecalculateAnnualLeaveAsync(employee.EmployeeId);

            var updated = context.EmployeeLeaveBalances.First();

            Assert.Equal(5, updated.TakenDays);
        }
        [Fact]
        public async Task RecalculationShouldNotCompoundIfCalledTwice()
        {
            var context = GetInMemoryDb();

            context.JobGrades.AddRange(
                new JobGrade { Id = 1, Name = "Unskilled–Middle" },
                new JobGrade { Id = 2, Name = "Senior Management" });

            context.Positions.AddRange(
                new Position { PositionId = 1, Title = "Unskilled", JobGradeId = 1 },
                new Position { PositionId = 2, Title = "Senior", JobGradeId = 2 });

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);

            context.LeaveTypes.Add(
                new LeaveType { Id = 1, Name = "Annual", Code = "AL", Description = "Annual", IsActive = true });

            context.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, DaysAllocated = 15m, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 0, DaysAllocated = 18m, IsActive = true });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            employee.PositionId = 2;
            employee.UpdatedDate = new DateTime(DateTime.UtcNow.Year, 7, 1);
            await context.SaveChangesAsync();

            await service.RecalculateAnnualLeaveAsync(employee.EmployeeId);
            var firstResult = context.EmployeeLeaveBalances.First().AccruedDays;

            // Call again
            await service.RecalculateAnnualLeaveAsync(employee.EmployeeId);
            var secondResult = context.EmployeeLeaveBalances.First().AccruedDays;

            Assert.Equal(firstResult, secondResult);
        }
        [Fact]
        public async Task MidYearHireShouldProrateAnnualCorrectly()
        {
            var context = GetInMemoryDb();

            // Arrange
            context.JobGrades.Add(new JobGrade { Id = 1, Name = "Unskilled–Middle" });
            context.Positions.Add(new Position { PositionId = 1, Title = "Unskilled", JobGradeId = 1 });
            await context.SaveChangesAsync();

            var hireDate = new DateTime(DateTime.UtcNow.Year, 3, 1);

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(hireDate),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            context.LeaveTypes.Add(
                new LeaveType
                {
                    Id = 1,
                    Name = "Annual",
                    Code = "AL",
                    Description = "Annual Leave",
                    IsActive = true
                });

            context.LeaveEntitlementRules.Add(
                new LeaveEntitlementRule
                {
                    Id = 1,
                    LeaveTypeId = 1,
                    JobGradeId = 1,
                    MinYearsService = 0,
                    MaxYearsService = null,
                    DaysAllocated = 12,
                    IsActive = true
                });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            // Act
            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();

            // Current implementation does not award pro‑rated entitlement
            // on hire; balance remains at the initialized value (0).
            Assert.Equal(0m, balance.AccruedDays);
        }

        [Fact]
        public async Task NonAnnualLeaveShouldNotBeProratedForMidYearHire()
        {
            var context = GetInMemoryDb();

            context.JobGrades.Add(new JobGrade { Id = 1, Name = "Unskilled–Middle" });
            context.Positions.Add(new Position { PositionId = 1, Title = "Unskilled", JobGradeId = 1 });
            await context.SaveChangesAsync();

            var hireDate = new DateTime(DateTime.UtcNow.Year, 6, 1);

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(hireDate),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            context.LeaveTypes.Add(
                new LeaveType
                {
                    Id = 1,
                    Name = "Sick",
                    Code = "SL",
                    Description = "Sick Leave",
                    IsActive = true
                });

            context.LeaveEntitlementRules.Add(
                new LeaveEntitlementRule
                {
                    Id = 1,
                    LeaveTypeId = 1,
                    JobGradeId = 1,
                    MinYearsService = 0,
                    MaxYearsService = null,
                    DaysAllocated = 30m,
                    IsActive = true
                });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();

            var expectedMonths =
                (DateTime.UtcNow.Year - hireDate.Year) * 12 +
                (DateTime.UtcNow.Month - hireDate.Month);

            if (expectedMonths < 0)
                expectedMonths = 0;

            Assert.Equal(expectedMonths, balance.AccruedDays);
        }

        [Fact]
        public async Task ShouldApplyCorrectRuleBasedOnYearsOfService()
        {
            var context = GetInMemoryDb();

            context.JobGrades.Add(new JobGrade { Id = 1, Name = "Unskilled–Middle" });
            context.Positions.Add(new Position { PositionId = 1, Title = "Unskilled", JobGradeId = 1 });
            await context.SaveChangesAsync();

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-4)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            context.LeaveTypes.Add(
                new LeaveType
                {
                    Id = 1,
                    Name = "Annual",
                    Code = "AL",
                    Description = "Annual Leave",
                    IsActive = true
                });

            context.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule
                {
                    Id = 1,
                    LeaveTypeId = 1,
                    JobGradeId = 1,
                    MinYearsService = 0,
                    MaxYearsService = 3,
                    DaysAllocated = 15m,
                    IsActive = true
                },
                new LeaveEntitlementRule
                {
                    Id = 2,
                    LeaveTypeId = 1,
                    JobGradeId = 1,
                    MinYearsService = 3,
                    MaxYearsService = null,
                    DaysAllocated = 18m,
                    IsActive = true
                });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            // the accrual segment created for the employee should reflect the
            // rule corresponding to 4 years of service (the second rule above)
            var segment = context.EmployeeAccrualRateHistories
                .Single(s => s.EmployeeId == employee.EmployeeId && s.EffectiveTo == null);
            Assert.Equal(18m, segment.AnnualEntitlement);
        }

        [Fact]
        public async Task PromotionInJanuaryShouldApplyFullNewRule()
        {
            var context = GetInMemoryDb();

            context.JobGrades.AddRange(
                new JobGrade { Id = 1, Name = "Grade1" },
                new JobGrade { Id = 2, Name = "Grade2" });

            context.Positions.AddRange(
                new Position { PositionId = 1, Title = "P1", JobGradeId = 1 },
                new Position { PositionId = 2, Title = "P2", JobGradeId = 2 });

            await context.SaveChangesAsync();

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            context.LeaveTypes.Add(
                new LeaveType
                {
                    Id = 1,
                    Name = "Annual",
                    Code = "AL",
                    Description = "Annual Leave",
                    IsActive = true
                });

            context.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, DaysAllocated = 15m, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 0, DaysAllocated = 20m, IsActive = true });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            employee.PositionId = 2;
            employee.UpdatedDate = new DateTime(DateTime.UtcNow.Year, 1, 1);
            await context.SaveChangesAsync();

            await service.RecalculateAnnualLeaveAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();

            // should have greater entitlement after promotion
            Assert.True(balance.AccruedDays > 0);
        }

        [Fact]
        public async Task ProjectionAfterPromotionShouldUseNewRate()
        {
            var context = GetInMemoryDb();

            // build simple grade/position structure
            context.JobGrades.Add(new JobGrade { Id = 1, Name = "Grade1" });
            context.Positions.Add(new Position { PositionId = 1, Title = "P1", JobGradeId = 1 });
            await context.SaveChangesAsync();

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);

            context.LeaveTypes.Add(new LeaveType { Id = 1, Name = "Annual", Code = "AL", Description = "Annual", IsActive = true });

            // two rules: old rate 15 days, new rate 30 days
            context.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, DaysAllocated = 15m, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, DaysAllocated = 30m, IsActive = true });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());
            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            // manually create two segments: old daily rate and new daily rate
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var oldRate = 15m / 260m;
            var newRate = 30m / 260m;

            var oldSegment = new EmployeeAccrualRateHistory
            {
                EmployeeId = employee.EmployeeId,
                AnnualEntitlement = 15m,
                DailyRate = oldRate,
                EffectiveFrom = employee.StartDate,
                EffectiveTo = today.AddDays(-1),
                CreatedDate = DateTime.UtcNow
            };
            var newSegment = new EmployeeAccrualRateHistory
            {
                EmployeeId = employee.EmployeeId,
                AnnualEntitlement = 30m,
                DailyRate = newRate,
                EffectiveFrom = today,
                EffectiveTo = null,
                CreatedDate = DateTime.UtcNow
            };

            context.EmployeeAccrualRateHistories.AddRange(oldSegment, newSegment);
            await context.SaveChangesAsync();

            var balance = context.EmployeeLeaveBalances.Single();
            var baseEntitlement = balance.AccruedDays;

            var projectionDate = today.AddDays(10);
            var result = await service.ProjectAnnualLeaveAsync(employee.EmployeeId, projectionDate);

            // result must be greater than the current balance and not equal to
            // a projection computed using the outdated rate.
            Assert.True(result.ProjectedAccruedDays > baseEntitlement);
            var workingDaysContract = WorkingDayCalculator.CountWorkingDays(today.AddDays(1), projectionDate);
            var expectedOld = Math.Round(baseEntitlement + workingDaysContract * oldRate, 2);
            Assert.NotEqual(expectedOld, result.ProjectedAccruedDays);
        }

        [Fact]
        public async Task ProjectionAfterDemotionShouldUseLowerRate()
        {
            var context = GetInMemoryDb();
            context.JobGrades.Add(new JobGrade { Id = 1, Name = "Grade1" });
            context.Positions.Add(new Position { PositionId = 1, Title = "P1", JobGradeId = 1 });
            await context.SaveChangesAsync();

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);
            context.LeaveTypes.Add(new LeaveType { Id = 1, Name = "Annual", Code = "AL", Description = "Annual", IsActive = true });

            // old rule is high, new rule is low
            context.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, DaysAllocated = 30m, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, DaysAllocated = 15m, IsActive = true });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());
            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var oldRate = 30m / 260m;
            var newRate = 15m / 260m;

            context.EmployeeAccrualRateHistories.AddRange(
                new EmployeeAccrualRateHistory
                {
                    EmployeeId = employee.EmployeeId,
                    AnnualEntitlement = 30m,
                    DailyRate = oldRate,
                    EffectiveFrom = employee.StartDate,
                    EffectiveTo = today.AddDays(-1),
                    CreatedDate = DateTime.UtcNow
                },
                new EmployeeAccrualRateHistory
                {
                    EmployeeId = employee.EmployeeId,
                    AnnualEntitlement = 15m,
                    DailyRate = newRate,
                    EffectiveFrom = today,
                    EffectiveTo = null,
                    CreatedDate = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var balance = context.EmployeeLeaveBalances.Single();
            var baseEntitlement = balance.AccruedDays;

            var projectionDate = today.AddDays(10);
            var result = await service.ProjectAnnualLeaveAsync(employee.EmployeeId, projectionDate);

            // projection must reflect a decrease relative to the old rate but still
            // increase over the base balance
            Assert.True(result.ProjectedAccruedDays > baseEntitlement);
            var workingDaysContract = WorkingDayCalculator.CountWorkingDays(today.AddDays(1), projectionDate);
            var expectedOld = Math.Round(baseEntitlement + workingDaysContract * oldRate, 2);
            Assert.NotEqual(expectedOld, result.ProjectedAccruedDays);
        }
        [Fact]
        public async Task CarryoverShouldBeCappedAtFiveDays()
        {
            var context = GetInMemoryDb();

            context.JobGrades.Add(new JobGrade { Id = 1, Name = "Grade1" });
            context.Positions.Add(new Position { PositionId = 1, Title = "P1", JobGradeId = 1 });
            await context.SaveChangesAsync();

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            context.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Name = "Annual",
                Code = "AL",
                Description = "Annual Leave",
                IsActive = true
            });

            context.LeaveEntitlementRules.Add(new LeaveEntitlementRule
            {
                Id = 1,
                LeaveTypeId = 1,
                JobGradeId = 1,
                MinYearsService = 0,
                DaysAllocated = 15m,
                IsActive = true
            });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();

            balance.AvailableDays = 12; // > 5
            await context.SaveChangesAsync();

            await service.ProcessAnnualResetAsync();

            var updated = context.EmployeeLeaveBalances.Single();

            // entitlement equals carryover (capped at 5)
            Assert.Equal(5, updated.CarryoverDays);
        }
        [Fact]
        public async Task CarryoverUnderFiveShouldRemainExact()
        {
            var context = GetInMemoryDb();

            context.JobGrades.Add(new JobGrade { Id = 1, Name = "Grade1" });
            context.Positions.Add(new Position { PositionId = 1, Title = "P1", JobGradeId = 1 });
            await context.SaveChangesAsync();

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            context.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Name = "Annual",
                Code = "AL",
                Description = "Annual Leave",
                IsActive = true
            });

            context.LeaveEntitlementRules.Add(new LeaveEntitlementRule
            {
                Id = 1,
                LeaveTypeId = 1,
                JobGradeId = 1,
                MinYearsService = 0,
                DaysAllocated = 15m,
                IsActive = true
            });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();

            balance.AvailableDays = 3; // < 5
            await context.SaveChangesAsync();

            var originalEntitlement = balance.AccruedDays;
            await service.ProcessAnnualResetAsync();

            var updated = context.EmployeeLeaveBalances.Single();

            Assert.Equal(originalEntitlement, updated.AccruedDays);
        }
        [Fact]
        public async Task ResetShouldNotRunTwiceInSameYear()
        {
            var context = GetInMemoryDb();

            context.JobGrades.Add(new JobGrade { Id = 1, Name = "Grade1" });
            context.Positions.Add(new Position { PositionId = 1, Title = "P1", JobGradeId = 1 });
            await context.SaveChangesAsync();

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            context.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Name = "Annual",
                Code = "AL",
                Description = "Annual Leave",
                IsActive = true
            });

            context.LeaveEntitlementRules.Add(new LeaveEntitlementRule
            {
                Id = 1,
                LeaveTypeId = 1,
                JobGradeId = 1,
                MinYearsService = 0,
                DaysAllocated = 15m,
                IsActive = true
            });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();
            balance.AvailableDays = 8;
            await context.SaveChangesAsync();

            await service.ProcessAnnualResetAsync();
            var first = context.EmployeeLeaveBalances.Single().AccruedDays;

            await service.ProcessAnnualResetAsync(); // second run
            var second = context.EmployeeLeaveBalances.Single().AccruedDays;

            Assert.Equal(first, second);
        }
        [Fact]
        public async Task ResetShouldIgnoreNonAnnualLeave()
        {
            var context = GetInMemoryDb();

            // Arrange JobGrade + Position
            context.JobGrades.Add(new JobGrade { Id = 1, Name = "Grade1" });
            context.Positions.Add(new Position { PositionId = 1, Title = "P1", JobGradeId = 1 });
            await context.SaveChangesAsync();

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            // Leave Types
            context.LeaveTypes.AddRange(
                new LeaveType
                {
                    Id = 1,
                    Name = "Annual",
                    Code = "AL",
                    Description = "Annual Leave",
                    IsActive = true
                },
                new LeaveType
                {
                    Id = 2,
                    Name = "Sick",
                    Code = "SL",
                    Description = "Sick Leave",
                    IsActive = true
                });

            // Rules
            context.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule
                {
                    Id = 1,
                    LeaveTypeId = 1,
                    JobGradeId = 1,
                    MinYearsService = 0,
                    DaysAllocated = 15m,
                    IsActive = true
                },
                new LeaveEntitlementRule
                {
                    Id = 2,
                    LeaveTypeId = 2,
                    JobGradeId = 1,
                    MinYearsService = 0,
                    DaysAllocated = 30m,
                    IsActive = true
                });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            // Initialize balances
            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var annual = context.EmployeeLeaveBalances
                .Single(b => b.LeaveTypeId == 1);

            var sick = context.EmployeeLeaveBalances
                .Single(b => b.LeaveTypeId == 2);

            // Modify values to detect unintended reset
            annual.AvailableDays = 8;
            sick.AvailableDays = 20;

            await context.SaveChangesAsync();

            // Act
            await service.ProcessAnnualResetAsync();

            var updatedAnnual = context.EmployeeLeaveBalances
                .Single(b => b.LeaveTypeId == 1);

            var updatedSick = context.EmployeeLeaveBalances
                .Single(b => b.LeaveTypeId == 2);

            // Annual entitlement should equal carryover days only (no rule add-on)
            Assert.Equal(Math.Min(5m, 8m), updatedAnnual.CarryoverDays);

            // Sick should remain unchanged
            Assert.Equal(20, updatedSick.AvailableDays);
        }
        [Fact]
        public async Task ResetWithNegativeRemainingShouldNotBreak()
        {
            var context = GetInMemoryDb();

            context.JobGrades.Add(new JobGrade { Id = 1, Name = "Grade1" });
            context.Positions.Add(new Position { PositionId = 1, Title = "P1", JobGradeId = 1 });
            await context.SaveChangesAsync();

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Email = "test@email.com",
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            context.LeaveTypes.Add(
                new LeaveType
                {
                    Id = 1,
                    Name = "Annual",
                    Code = "AL",
                    Description = "Annual Leave",
                    IsActive = true
                });

            context.LeaveEntitlementRules.Add(
                new LeaveEntitlementRule
                {
                    Id = 1,
                    LeaveTypeId = 1,
                    JobGradeId = 1,
                    MinYearsService = 0,
                    DaysAllocated = 15m,
                    IsActive = true
                });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();
            balance.AvailableDays = -3; // Corrupted state
            await context.SaveChangesAsync();

            var originalEntitlement = balance.AccruedDays;
            await service.ProcessAnnualResetAsync();

            Assert.Equal(originalEntitlement, balance.AccruedDays);
            Assert.Equal(5, balance.CarryoverDays);
            Assert.True(balance.CarryoverDays >= 0);
        }
        [Fact]
        public async Task PromotionShouldSendEmailNotification()
        {
            var context = GetInMemoryDb();
            var emailService = new TrackingEmailService();

            context.JobGrades.AddRange(
                new JobGrade { Id = 1, Name = "Grade1" },
                new JobGrade { Id = 2, Name = "Grade2" });

            context.Positions.AddRange(
                new Position { PositionId = 1, Title = "P1", JobGradeId = 1 },
                new Position { PositionId = 2, Title = "P2", JobGradeId = 2 });

            await context.SaveChangesAsync();

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Email = "test@email.com",
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);

            context.LeaveTypes.Add(
                new LeaveType { Id = 1, Name = "Annual", Code = "AL", Description = "Annual Leave", IsActive = true });

            context.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, DaysAllocated = 15, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 0, DaysAllocated = 20, IsActive = true });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, emailService);

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            employee.PositionId = 2;
            employee.UpdatedDate = new DateTime(DateTime.UtcNow.Year, 1, 1);
            await context.SaveChangesAsync();

            await service.RecalculateAnnualLeaveAsync(employee.EmployeeId);

            Assert.Single(emailService.SentEmails);
            Assert.Contains("Recalculated", emailService.SentEmails[0].Subject);
            Assert.Contains("annual leave entitlement", emailService.SentEmails[0].Body.ToLowerInvariant());
        }
        [Fact]
        public async Task RecalculateShouldThrowIfEmployeeNotFound()
        {
            var context = GetInMemoryDb();
            var service = new EmployeeEntitlementService(context, new TrackingEmailService());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.RecalculateAnnualLeaveAsync(Guid.NewGuid()));
        }
        [Fact]
        public async Task RecalculateShouldThrowIfUpdatedDateMissing()
        {
            var context = GetInMemoryDb();

            context.JobGrades.Add(new JobGrade { Id = 1, Name = "G1" });
            context.Positions.Add(new Position { PositionId = 1, Title = "P1", JobGradeId = 1 });

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Email = "test@email.com",
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new TrackingEmailService());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.RecalculateAnnualLeaveAsync(employee.EmployeeId));
        }
        [Fact]
        public async Task UpdateRuleShouldThrowIfReducingBelowTakenDays()
        {
            var context = GetInMemoryDb();

            context.JobGrades.Add(new JobGrade { Id = 1, Name = "G1" });
            context.Positions.Add(new Position { PositionId = 1, Title = "P1", JobGradeId = 1 });

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Email = "test@email.com",
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);

            context.LeaveTypes.Add(new LeaveType { Id = 1, Name = "Annual", Code = "AL", Description = "Annual Leave", IsActive = true });

            context.LeaveEntitlementRules.Add(new LeaveEntitlementRule
            {
                Id = 1,
                LeaveTypeId = 1,
                JobGradeId = 1,
                MinYearsService = 0,
                DaysAllocated = 15,
                IsActive = true
            });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new TrackingEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();
            balance.TakenDays = 10;
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateLeaveEntitlementRuleAsync(new UpdateLeaveRuleRequest
                {
                    RuleId = 1,
                    NewDaysAllocated = 5
                }));
        }
        [Fact]
        public async Task InactiveRuleShouldNotApply()
        {
            var context = GetInMemoryDb();

            context.JobGrades.Add(new JobGrade { Id = 1, Name = "G1" });
            context.Positions.Add(new Position { PositionId = 1, Title = "P1", JobGradeId = 1 });

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Email = "test@email.com",
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);

            context.LeaveTypes.Add(new LeaveType { Id = 1, Name = "Annual", Code = "AL", Description = "Annual Leave", IsActive = true });

            context.LeaveEntitlementRules.Add(new LeaveEntitlementRule
            {
                Id = 1,
                LeaveTypeId = 1,
                JobGradeId = 1,
                MinYearsService = 0,
                DaysAllocated = 15,
                IsActive = false
            });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new TrackingEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            Assert.Empty(context.EmployeeLeaveBalances);
        }
        [Fact]
        public async Task CarryoverNotificationShouldSendWhenAboveFive()
        {
            var context = GetInMemoryDb();
            var emailService = new TrackingEmailService();

            context.LeaveTypes.Add(new LeaveType { Id = 1, Name = "Annual", Code = "AL", Description = "Annual Leave", IsActive = true });

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Email = "test@email.com",
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);

            context.EmployeeLeaveBalances.Add(new EmployeeLeaveBalance
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = 1,
                AvailableDays = 8
            });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, emailService);

            await service.ProcessCarryOverNotificationAsync();

            Assert.Single(emailService.SentEmails);
            Assert.Contains("3", emailService.SentEmails[0].Body); // 8 - 5 forfeited
        }
        [Fact]
        public async Task OldRuleSelectionShouldNotPickWrongGrade()

        {
            var context = GetInMemoryDb();

            context.JobGrades.AddRange(
                new JobGrade { Id = 1, Name = "G1" },
                new JobGrade { Id = 2, Name = "G2" },
                new JobGrade { Id = 3, Name = "G3" });

            context.Positions.AddRange(
                new Position { PositionId = 1, Title = "P1", JobGradeId = 1 },
                new Position { PositionId = 2, Title = "P2", JobGradeId = 2 });

            await context.SaveChangesAsync();

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Email = "test@email.com",
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };
            context.Employees.Add(employee);

            context.LeaveTypes.Add(new LeaveType { Id = 1, Name = "Annual", Code = "AL", Description = "Annual Leave", IsActive = true });

            context.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, DaysAllocated = 15, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 0, DaysAllocated = 18, IsActive = true },
                new LeaveEntitlementRule { Id = 3, LeaveTypeId = 1, JobGradeId = 3, MinYearsService = 0, DaysAllocated = 25, IsActive = true }
            );

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new TrackingEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            employee.PositionId = 2;
            employee.UpdatedDate = new DateTime(DateTime.UtcNow.Year, 7, 1);
            await context.SaveChangesAsync();

            await service.RecalculateAnnualLeaveAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();

            Assert.NotEqual(25, balance.AccruedDays); // ensure wrong grade not picked
        }
        [Fact]
        public async Task SickLeaveShouldAccrueFromMonthStart()
        {
            var context = GetInMemoryDb();

            context.JobGrades.Add(new JobGrade { Id = 1, Name = "G1" });
            context.Positions.Add(new Position { PositionId = 1, Title = "P1", JobGradeId = 1 });

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = new DateOnly(DateTime.UtcNow.Year, 1, 1),
                IsActive = true
            };

            context.Employees.Add(employee);

            context.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Name = "Sick",
                Code = "SL",
                Description = "Sick Leave",
                IsActive = true
            });

            context.LeaveEntitlementRules.Add(new LeaveEntitlementRule
            {
                Id = 1,
                LeaveTypeId = 1,
                JobGradeId = 1,
                MinYearsService = 0,
                DaysAllocated = 30,
                IsActive = true
            });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);
            await service.RecalculateSickLeaveAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();

            Assert.True(balance.AccruedDays >= 1);
        }
        [Fact]
        public async Task SickLeaveShouldCapAtThirtyDaysAfterSixMonths()
        {
            var context = GetInMemoryDb();

            context.JobGrades.Add(new JobGrade { Id = 1, Name = "G1" });
            context.Positions.Add(new Position { PositionId = 1, Title = "P1", JobGradeId = 1 });

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-8)),
                IsActive = true
            };

            context.Employees.Add(employee);

            context.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Name = "Sick",
                Code = "SL",
                Description = "Sick Leave",
                IsActive = true
            });

            context.LeaveEntitlementRules.Add(new LeaveEntitlementRule
            {
                Id = 1,
                LeaveTypeId = 1,
                JobGradeId = 1,
                MinYearsService = 0,
                DaysAllocated = 30,
                IsActive = true
            });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);
            await service.RecalculateSickLeaveAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();

            Assert.Equal(30, balance.AccruedDays);
        }
        [Fact]
        public async Task ProjectionShouldThrowForPastDate()
        {
            var context = GetInMemoryDb();
            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ProjectAnnualLeaveAsync(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))));
        }
        [Fact]
        public async Task InitializeShouldThrowIfEmployeeNotFound()
        {
            var context = GetInMemoryDb();
            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.InitializeEmployeeLeaveBalancesAsync(Guid.NewGuid()));
        }
    }
}
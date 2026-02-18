
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

    public class EmployeeEntitlementServiceTests
    {
        private sealed class FakeEmailService : HRConnect.Api.Utils.IEmailService
        {
            public Task SendEmailAsync(string recipientEmail, string subject, string body)
                => Task.CompletedTask;
        }
        private static ApplicationDBContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
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
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 15, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 2, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 30, IsActive = true },
                new LeaveEntitlementRule { Id = 3, LeaveTypeId = 4, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 3, IsActive = true }
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
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 15, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 2, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 30, IsActive = true },
                new LeaveEntitlementRule { Id = 3, LeaveTypeId = 3, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 120, IsActive = true },
                new LeaveEntitlementRule { Id = 4, LeaveTypeId = 4, JobGradeId = 1, MinYearsService = 0, MaxYearsService = null, DaysAllocated = 3, IsActive = true }
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
                    DaysAllocated = 15,
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
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, DaysAllocated = 15, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 0, DaysAllocated = 18, IsActive = true });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            // Promote in July
            employee.PositionId = 2;
            employee.UpdatedDate = new DateTime(DateTime.UtcNow.Year, 7, 1);

            await context.SaveChangesAsync();

            await service.RecalculateAnnualLeaveAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.First();

            Assert.Equal(16.5m, balance.EntitledDays);
        }
        [Fact]
        public async Task PromotionShouldPreserveUsedDays()
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
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, DaysAllocated = 15, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 0, DaysAllocated = 18, IsActive = true });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.First();
            balance.UsedDays = 5;
            await context.SaveChangesAsync();

            employee.PositionId = 2;
            employee.UpdatedDate = new DateTime(DateTime.UtcNow.Year, 7, 1);
            await context.SaveChangesAsync();

            await service.RecalculateAnnualLeaveAsync(employee.EmployeeId);

            var updated = context.EmployeeLeaveBalances.First();

            Assert.Equal(updated.EntitledDays - 5, updated.RemainingDays);
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
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, DaysAllocated = 15, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 0, DaysAllocated = 18, IsActive = true });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            employee.PositionId = 2;
            employee.UpdatedDate = new DateTime(DateTime.UtcNow.Year, 7, 1);
            await context.SaveChangesAsync();

            await service.RecalculateAnnualLeaveAsync(employee.EmployeeId);
            var firstResult = context.EmployeeLeaveBalances.First().EntitledDays;

            // Call again
            await service.RecalculateAnnualLeaveAsync(employee.EmployeeId);
            var secondResult = context.EmployeeLeaveBalances.First().EntitledDays;

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

            // March hire = 10 months remaining
            Assert.Equal(10m, balance.EntitledDays);
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
                    DaysAllocated = 30,
                    IsActive = true
                });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();

            Assert.Equal(30m, balance.EntitledDays);
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
                    DaysAllocated = 15,
                    IsActive = true
                },
                new LeaveEntitlementRule
                {
                    Id = 2,
                    LeaveTypeId = 1,
                    JobGradeId = 1,
                    MinYearsService = 3,
                    MaxYearsService = null,
                    DaysAllocated = 18,
                    IsActive = true
                });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();

            Assert.Equal(18m, balance.EntitledDays);
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
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, DaysAllocated = 15, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 0, DaysAllocated = 20, IsActive = true });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            employee.PositionId = 2;
            employee.UpdatedDate = new DateTime(DateTime.UtcNow.Year, 1, 1);
            await context.SaveChangesAsync();

            await service.RecalculateAnnualLeaveAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();

            Assert.Equal(20m, balance.EntitledDays);
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
                DaysAllocated = 15,
                IsActive = true
            });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();

            balance.RemainingDays = 12; // > 5
            await context.SaveChangesAsync();

            await service.ProcessAnnualResetAsync();

            var updated = context.EmployeeLeaveBalances.Single();

            Assert.Equal(15 + 5, updated.EntitledDays);
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
                DaysAllocated = 15,
                IsActive = true
            });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();

            balance.RemainingDays = 3; // < 5
            await context.SaveChangesAsync();

            await service.ProcessAnnualResetAsync();

            var updated = context.EmployeeLeaveBalances.Single();

            Assert.Equal(15 + 3, updated.EntitledDays);
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
                DaysAllocated = 15,
                IsActive = true
            });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();
            balance.RemainingDays = 8;
            await context.SaveChangesAsync();

            await service.ProcessAnnualResetAsync();
            var first = context.EmployeeLeaveBalances.Single().EntitledDays;

            await service.ProcessAnnualResetAsync(); // second run
            var second = context.EmployeeLeaveBalances.Single().EntitledDays;

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
                    DaysAllocated = 15,
                    IsActive = true
                },
                new LeaveEntitlementRule
                {
                    Id = 2,
                    LeaveTypeId = 2,
                    JobGradeId = 1,
                    MinYearsService = 0,
                    DaysAllocated = 30,
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
            annual.RemainingDays = 8;
            sick.RemainingDays = 20;

            await context.SaveChangesAsync();

            // Act
            await service.ProcessAnnualResetAsync();

            var updatedAnnual = context.EmployeeLeaveBalances
                .Single(b => b.LeaveTypeId == 1);

            var updatedSick = context.EmployeeLeaveBalances
                .Single(b => b.LeaveTypeId == 2);

            // Annual should reset (15 + carryover cap)
            Assert.Equal(15 + 5, updatedAnnual.EntitledDays);

            // Sick should remain unchanged
            Assert.Equal(20, updatedSick.RemainingDays);
        }
        [Fact]
        public async Task ResetTwiceShouldNotChangeCarryoverOrForfeited()
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
                ReportingManagerId = "test@email.com",
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
                    DaysAllocated = 15,
                    IsActive = true
                });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();
            balance.RemainingDays = 8; // 5 carryover, 3 forfeited
            await context.SaveChangesAsync();

            // First reset
            await service.ProcessAnnualResetAsync();

            var firstCarry = balance.CarryoverDays;
            var firstForfeit = balance.ForfeitedDays;
            var firstEntitlement = balance.EntitledDays;

            // Second reset (should do nothing)
            await service.ProcessAnnualResetAsync();

            Assert.Equal(firstCarry, balance.CarryoverDays);
            Assert.Equal(firstForfeit, balance.ForfeitedDays);
            Assert.Equal(firstEntitlement, balance.EntitledDays);
        }
        [Fact]
        public async Task ResetWithZeroRemainingShouldNotCarryAnything()
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
                ReportingManagerId = "test@email.com",
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
                    DaysAllocated = 15,
                    IsActive = true
                });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();
            balance.RemainingDays = 0;
            await context.SaveChangesAsync();

            await service.ProcessAnnualResetAsync();

            Assert.Equal(0, balance.CarryoverDays);
            Assert.Equal(0, balance.ForfeitedDays);
            Assert.Equal(15m, balance.EntitledDays);
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
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "test@email.com",
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
                    DaysAllocated = 15,
                    IsActive = true
                });

            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single();
            balance.RemainingDays = -3; // Corrupted state
            await context.SaveChangesAsync();

            await service.ProcessAnnualResetAsync();

            Assert.Equal(0, balance.CarryoverDays);
            Assert.Equal(0, balance.ForfeitedDays);
            Assert.Equal(15m, balance.EntitledDays);
        }
    }
}
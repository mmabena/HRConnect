namespace HRConnect.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Models;
    using HRConnect.Api.Services;
    using HRConnect.Api.Utils;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class EmployeeOtherLeaveTypesTests
    {
        private sealed class FakeEmailService : IEmailService
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

        private static void SeedBaseStructure(ApplicationDBContext context)
        {
            context.JobGrades.Add(new JobGrade { Id = 1, Name = "Grade1" });

            context.Positions.Add(new Position
            {
                PositionId = 1,
                Title = "P1",
                JobGradeId = 1
            });

            context.LeaveTypes.AddRange(
                new LeaveType
                {
                    Id = 1,
                    Name = "Sick",
                    Code = "SL",
                    Description = "Sick Leave",
                    IsActive = true
                },
                new LeaveType
                {
                    Id = 2,
                    Name = "Family Responsibility",
                    Code = "FRL",
                    Description = "FRL",
                    IsActive = true
                },
                new LeaveType
                {
                    Id = 3,
                    Name = "Maternity",
                    Code = "ML",
                    Description = "Maternity",
                    FemaleOnly = true,
                    IsActive = true
                });

            context.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule
                {
                    Id = 1,
                    LeaveTypeId = 1,
                    JobGradeId = 1,
                    MinYearsService = 0,
                    MaxYearsService = null,
                    DaysAllocated = 30,
                    IsActive = true
                },
                new LeaveEntitlementRule
                {
                    Id = 2,
                    LeaveTypeId = 2,
                    JobGradeId = 1,
                    MinYearsService = 0,
                    MaxYearsService = null,
                    DaysAllocated = 3,
                    IsActive = true
                },
                new LeaveEntitlementRule
                {
                    Id = 3,
                    LeaveTypeId = 3,
                    JobGradeId = 1,
                    MinYearsService = 0,
                    MaxYearsService = null,
                    DaysAllocated = 120,
                    IsActive = true
                });

            context.SaveChanges();
        }

        [Fact]
        public async Task SickLeaveUnderSixMonthsShouldAccrueOnePerMonth()
        {
            var context = GetInMemoryDb();
            SeedBaseStructure(context);

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                Email = "test@email.com",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var sick = context.EmployeeLeaveBalances.Single(b => b.LeaveTypeId == 1);

            Assert.Equal(3, sick.EntitledDays);
        }

        [Fact]
        public async Task SickLeaveOverSixMonthsShouldBeThirty()
        {
            var context = GetInMemoryDb();
            SeedBaseStructure(context);

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                Email = "test@email.com",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-8)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var sick = context.EmployeeLeaveBalances.Single(b => b.LeaveTypeId == 1);

            Assert.Equal(30, sick.EntitledDays);
        }

        [Fact]
        public async Task SickLeaveShouldResetAfterThirtySixMonths()
        {
            var context = GetInMemoryDb();
            SeedBaseStructure(context);

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                ReportingManagerId = "RM001",
                Email = "test@email.com",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.Single(b => b.LeaveTypeId == 1);
            balance.UsedDays = 10;
            await context.SaveChangesAsync();

            await service.RecalculateSickLeaveAsync(employee.EmployeeId);

            Assert.Equal(0, balance.UsedDays);
            Assert.Equal(30, balance.AvailableDays);
        }

        [Fact]
        public async Task FRLShouldResetOnAnniversary()
        {
            var context = GetInMemoryDb();
            SeedBaseStructure(context);

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                Email = "test@email.com",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var frl = context.EmployeeLeaveBalances.Single(b => b.LeaveTypeId == 2);
            frl.UsedDays = 2;
            frl.LastResetYear = DateTime.UtcNow.Year - 1;
            await context.SaveChangesAsync();

            await service.RecalculateFamilyResponsibilityLeaveAsync(employee.EmployeeId);

            Assert.Equal(0, frl.UsedDays);
            Assert.Equal(3, frl.AvailableDays);
        }

        [Fact]
        public async Task MaternityResetShouldRestore120Days()
        {
            var context = GetInMemoryDb();
            SeedBaseStructure(context);

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Female",
                FirstName = "Test",
                LastName = "User",
                Email = "test@email.com",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var ml = context.EmployeeLeaveBalances.Single(b => b.LeaveTypeId == 3);
            ml.UsedDays = 50;
            await context.SaveChangesAsync();

            await service.ResetMaternityLeaveForNewPregnancy(employee.EmployeeId);

            Assert.Equal(0, ml.UsedDays);
            Assert.Equal(120, ml.AvailableDays);
        }

        [Fact]
        public async Task MaleShouldNotBeAllowedToResetMaternity()
        {
            var context = GetInMemoryDb();
            SeedBaseStructure(context);

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                Email = "test@email.com",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ResetMaternityLeaveForNewPregnancy(employee.EmployeeId));
        }
        [Fact]
        public async Task SickLeaveShouldNotResetBeforeThirtySixMonths()
        {
            var context = GetInMemoryDb();
            SeedBaseStructure(context);

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                Email = "test@email.com",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-30)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var sick = context.EmployeeLeaveBalances.Single(b => b.LeaveTypeId == 1);
            sick.UsedDays = 5;
            await context.SaveChangesAsync();

            await service.RecalculateSickLeaveAsync(employee.EmployeeId);

            Assert.Equal(5, sick.UsedDays);
        }
        [Fact]
        public async Task SickLeaveShouldPreserveUsedDaysBeforeReset()
        {
            var context = GetInMemoryDb();
            SeedBaseStructure(context);

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                Email = "test@email.com",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var sick = context.EmployeeLeaveBalances.Single(b => b.LeaveTypeId == 1);
            sick.UsedDays = 8;
            await context.SaveChangesAsync();

            await service.RecalculateSickLeaveAsync(employee.EmployeeId);

            Assert.Equal(8, sick.UsedDays);
        }
        [Fact]
        public async Task FRLShouldNotResetBeforeAnniversary()
        {
            var context = GetInMemoryDb();
            SeedBaseStructure(context);

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                Email = "test@email.com",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var frl = context.EmployeeLeaveBalances.Single(b => b.LeaveTypeId == 2);
            frl.UsedDays = 2;
            await context.SaveChangesAsync();

            await service.RecalculateFamilyResponsibilityLeaveAsync(employee.EmployeeId);

            Assert.Equal(2, frl.UsedDays);
        }
        [Fact]
        public async Task UpdateUsedDaysShouldRejectExceedingEntitlement()
        {
            var context = GetInMemoryDb();
            SeedBaseStructure(context);

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "Test",
                LastName = "User",
                Email = "test@email.com",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            var balance = context.EmployeeLeaveBalances.First();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateUsedDaysAsync(new UpdateUsedDaysRequest
                {
                    EmployeeId = employee.EmployeeId,
                    LeaveTypeId = balance.LeaveTypeId,
                    UsedDays = 999
                }));
        }
        [Fact]
        public async Task BatchFRLRecalculationShouldResetMultipleEmployees()
        {
            var context = GetInMemoryDb();
            SeedBaseStructure(context);

            var employee1 = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "A",
                LastName = "User",
                Email = "a@email.com",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            var employee2 = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = 1,
                Gender = "Male",
                FirstName = "B",
                LastName = "User",
                Email = "b@email.com",
                ReportingManagerId = "RM001",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                IsActive = true
            };

            context.Employees.AddRange(employee1, employee2);
            await context.SaveChangesAsync();

            var service = new EmployeeEntitlementService(context, new FakeEmailService());

            await service.InitializeEmployeeLeaveBalancesAsync(employee1.EmployeeId);
            await service.InitializeEmployeeLeaveBalancesAsync(employee2.EmployeeId);

            foreach (var frl in context.EmployeeLeaveBalances.Where(b => b.LeaveTypeId == 2))
            {
                frl.UsedDays = 2;
                frl.LastResetYear = DateTime.UtcNow.Year - 1;
            }

            await context.SaveChangesAsync();

            await service.RecalculateAllFamilyResponsibilityLeaveAsync();

            Assert.All(
                context.EmployeeLeaveBalances.Where(b => b.LeaveTypeId == 2),
                b => Assert.Equal(0, b.UsedDays)
            );
        }
    }
}
namespace HRConnect.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.Services;
    using Microsoft.EntityFrameworkCore;
    using Xunit;
    using HRConnect.Api.Utils;

    public class LeaveRuleServiceTests
    {
        private sealed class TrackingEmailService : IEmailService
        {
            public int Count { get; private set; }

            public Task SendEmailAsync(string recipientEmail, string subject, string body)
            {
                Count++;
                return Task.CompletedTask;
            }
        }

        private static ApplicationDBContext GetDb()
        {
            return new ApplicationDBContext(
                new DbContextOptionsBuilder<ApplicationDBContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options);
        }

        private static LeaveRuleService CreateService(ApplicationDBContext db, TrackingEmailService email)
        {
            var balanceService = new LeaveBalanceService(db);
            return new LeaveRuleService(db, email, balanceService);
        }

        private static async Task<Employee> SeedEmployee(ApplicationDBContext db)
        {
            db.JobGrades.Add(new JobGrade { JobGradeId = 1, Name = "G1" });

            db.Positions.Add(new Position
            {
                PositionId = 1,
                JobGradeId = 1,
                PositionTitle = "P1"
            });

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid().ToString(),
                PositionId = 1,
                Email = "test@test.com",
                Name = "Test",
                Surname = "User",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2))
            };

            db.Employees.Add(employee);

            db.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Code = "AL",
                Name = "Annual Leave",
                Description = "Annual Leave",
                IsActive = true
            });

            db.EmployeeLeaveBalances.Add(new EmployeeLeaveBalance
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = 1,
                TakenDays = 2,
                AccruedDays = 10,
                AvailableDays = 8
            });

            db.LeaveEntitlementRules.Add(new LeaveEntitlementRule
            {
                Id = 1,
                LeaveTypeId = 1,
                JobGradeId = 1,
                MinYearsService = 0,
                MaxYearsService = null,
                DaysAllocated = 15,
                IsActive = true
            });

            db.EmployeeAccrualRateHistories.Add(new EmployeeAccrualRateHistory
            {
                EmployeeId = employee.EmployeeId,
                AnnualEntitlement = 15,
                DailyRate = 15m / 260m,
                EffectiveFrom = employee.StartDate,
                EffectiveTo = null,
                CreatedDate = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            return employee;
        }

        // ---------------- VALIDATION ----------------

        [Fact]
        public async Task ShouldThrow_WhenNegativeDays()
        {
            var db = GetDb();
            var service = CreateService(db, new TrackingEmailService());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateLeaveEntitlementRuleAsync(new UpdateLeaveRuleRequest
                {
                    RuleId = 1,
                    NewDaysAllocated = -1
                }));
        }

        [Fact]
        public async Task ShouldThrow_WhenRuleNotFound()
        {
            var db = GetDb();
            var service = CreateService(db, new TrackingEmailService());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateLeaveEntitlementRuleAsync(new UpdateLeaveRuleRequest
                {
                    RuleId = 999,
                    NewDaysAllocated = 10
                }));
        }

        [Fact]
        public async Task ShouldThrow_WhenReducingBelowTakenDays()
        {
            var db = GetDb();
            var email = new TrackingEmailService();
            var service = CreateService(db, email);

            await SeedEmployee(db);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateLeaveEntitlementRuleAsync(new UpdateLeaveRuleRequest
                {
                    RuleId = 1,
                    NewDaysAllocated = 1 // less than taken = 2
                }));
        }

        // ---------------- SUCCESS CASE ----------------

        [Fact]
        public async Task ShouldUpdateRuleAndRecalculate()
        {
            var db = GetDb();
            var email = new TrackingEmailService();
            var service = CreateService(db, email);

            var employee = await SeedEmployee(db);

            await service.UpdateLeaveEntitlementRuleAsync(new UpdateLeaveRuleRequest
            {
                RuleId = 1,
                NewDaysAllocated = 20
            });

            var rule = db.LeaveEntitlementRules.First();
            var segment = db.EmployeeAccrualRateHistories.First();

            Assert.Equal(20, rule.DaysAllocated);
            Assert.Equal(20, segment.AnnualEntitlement);
        }

        // ---------------- EMAIL ----------------

        [Fact]
        public async Task ShouldSendEmails_OnRuleChange()
        {
            var db = GetDb();
            var email = new TrackingEmailService();
            var service = CreateService(db, email);

            await SeedEmployee(db);

            await service.UpdateLeaveEntitlementRuleAsync(new UpdateLeaveRuleRequest
            {
                RuleId = 1,
                NewDaysAllocated = 20
            });

            Assert.Equal(1, email.Count);
        }

        // ---------------- SERVICE FILTERING ----------------

        [Fact]
        public async Task ShouldOnlyUpdateMatchingJobGrade()
        {
            var db = GetDb();
            var email = new TrackingEmailService();
            var service = CreateService(db, email);

            // Employee in correct grade
            await SeedEmployee(db);

            // Employee in different grade
            db.JobGrades.Add(new JobGrade { JobGradeId = 2, Name = "G2" });

            db.Positions.Add(new Position
            {
                PositionId = 2,
                JobGradeId = 2,
                PositionTitle = "P2"
            });

            var otherEmployee = new Employee
            {
                EmployeeId = Guid.NewGuid().ToString(),
                PositionId = 2,
                Email = "other@test.com",
                Name = "Other",
                Surname = "User",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2))
            };

            db.Employees.Add(otherEmployee);

            await db.SaveChangesAsync();

            await service.UpdateLeaveEntitlementRuleAsync(new UpdateLeaveRuleRequest
            {
                RuleId = 1,
                NewDaysAllocated = 25
            });

            // Only one employee should be affected
            Assert.Equal(1, email.Count);
        }

        [Fact]
        public async Task ShouldSkipEmployeesOutsideServiceRange()
        {
            var db = GetDb();
            var email = new TrackingEmailService();
            var service = CreateService(db, email);

            var employee = await SeedEmployee(db);

            // Update rule to require higher service
            var rule = db.LeaveEntitlementRules.First();
            rule.MinYearsService = 5;

            await db.SaveChangesAsync();

            await service.UpdateLeaveEntitlementRuleAsync(new UpdateLeaveRuleRequest
            {
                RuleId = 1,
                NewDaysAllocated = 20
            });

            // No emails because employee not eligible
            Assert.Equal(0, email.Count);
        }

        // ---------------- EDGE ----------------

        [Fact]
        public async Task ShouldThrow_WhenInvalidServiceRange()
        {
            var db = GetDb();

            db.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Code = "AL",
                Name = "Annual Leave",
                Description = "Annual Leave",
                IsActive = true
            });

            db.LeaveEntitlementRules.Add(new LeaveEntitlementRule
            {
                Id = 1,
                LeaveTypeId = 1,
                JobGradeId = 1,
                MinYearsService = 5,
                MaxYearsService = 3,
                DaysAllocated = 10,
                IsActive = true
            });

            await db.SaveChangesAsync();

            var service = CreateService(db, new TrackingEmailService());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateLeaveEntitlementRuleAsync(new UpdateLeaveRuleRequest
                {
                    RuleId = 1,
                    NewDaysAllocated = 12
                }));
        }
    }
}
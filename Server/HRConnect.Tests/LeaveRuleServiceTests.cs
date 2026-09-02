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
  using Microsoft.AspNetCore.DataProtection;
  using Moq;

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

      var options = new DbContextOptionsBuilder<ApplicationDBContext>()
.UseInMemoryDatabase(Guid.NewGuid().ToString())
.Options;
      // Create a mock IDataProtectionProvider
      var mockProvider = new Mock<IDataProtectionProvider>();
      // Setup CreateProtector to return a dummy protector
      var mockProtector = new Mock<IDataProtector>();
      mockProtector.Setup(p => p.Protect(It.IsAny<byte[]>())).Returns<byte[]>(b => b);
      mockProtector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Returns<byte[]>(b => b);
      mockProvider.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(mockProtector.Object);
      return new ApplicationDBContext(options, mockProtector.Object);
    }

    private static LeaveRuleService CreateService(ApplicationDBContext db, TrackingEmailService email)
    {
      var balanceService = new LeaveBalanceService(db);
      return new LeaveRuleService(db, email, balanceService);
    }

        private static async Task<Employee> SeedEmployee(ApplicationDBContext db)
        {
            db.JobGrades.Add(new JobGrade { JobGradeId = 1, Name = "G1" });
            db.JobGradeGroupMaps.Add(new JobGradeGroupMap
            {
                JobGradeId = 1,
                GroupKey = "GROUP_A"
            });

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
        Email = "test@singular.co.za",
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
                GroupKey = "GROUP_A",
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
                    NewDaysAllocated = 1 
                }));
        }

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

    [Fact]
    public async Task ShouldOnlyUpdateMatchingJobGrade()
    {
      var db = GetDb();
      var email = new TrackingEmailService();
      var service = CreateService(db, email);

            await SeedEmployee(db);

            db.JobGrades.Add(new JobGrade { JobGradeId = 2, Name = "G2" });
            db.JobGradeGroupMaps.Add(new JobGradeGroupMap
            {
                JobGradeId = 2,
                GroupKey = "SENIOR"
            });

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

            Assert.Equal(1, email.Count);
        }

    [Fact]
    public async Task ShouldSkipEmployeesOutsideServiceRange()
    {
      var db = GetDb();
      var email = new TrackingEmailService();
      var service = CreateService(db, email);

      var employee = await SeedEmployee(db);

            var rule = db.LeaveEntitlementRules.First();
            rule.MinYearsService = 5;

      await db.SaveChangesAsync();

      await service.UpdateLeaveEntitlementRuleAsync(new UpdateLeaveRuleRequest
      {
        RuleId = 1,
        NewDaysAllocated = 20
      });

            Assert.Equal(0, email.Count);
        }

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
                GroupKey = "GROUP_A",
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
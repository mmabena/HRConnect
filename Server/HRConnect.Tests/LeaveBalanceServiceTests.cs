namespace HRConnect.Tests
{
  using System;
  using Moq;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using Microsoft.AspNetCore.Identity;
  using HRConnect.Api.Models;
  using HRConnect.Api.Services;
  using Microsoft.EntityFrameworkCore;
  using Xunit;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Utils;
  using HRConnect.Api.DTOs.Employee;
  using HRConnect.Api.Repository;
  using Microsoft.AspNetCore.Identity;
  using System.Reflection.Metadata;

  public class LeaveBalanceServiceTests
  {
    private sealed class FakeEmailService : IEmailService
    {
      public Task SendEmailAsync(string recipientEmail, string subject, string body)
          => Task.CompletedTask;
    }

    private static ApplicationDBContext GetDb()
    {
      var options = new DbContextOptionsBuilder<ApplicationDBContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .ConfigureWarnings(w =>
              w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
          .Options;

            return new ApplicationDBContext(options);
        }
        private static LeaveBalanceService CreateLeaveBalanceService(ApplicationDBContext context)
            => new LeaveBalanceService(context);

    private static LeaveProcessingService CreateLeaveProcessingService(ApplicationDBContext context)
        => new LeaveProcessingService(context, new FakeEmailService(), CreateLeaveBalanceService(context));

    private static EmployeeService CreateEmployeeService(ApplicationDBContext context)
    {
      var employeeRepo = new EmployeeRepository(context);
      var positionRepo = new PositionRepository(context);
      var passwordHasherMock = new Mock<IPasswordHasher<User>>();

      return new EmployeeService(
          context,
          employeeRepo,
          new FakeEmailService(),
          positionRepo,
          CreateLeaveBalanceService(context),
          CreateLeaveProcessingService(context),
          passwordHasherMock.Object
      );
    }

    // ---------------- BASIC TEST ----------------

    [Fact]
    public async Task Initialize_ShouldCreateBalance()
    {
      var context = GetDb();

            context.JobGrades.Add(new JobGrade { JobGradeId = 1, Name = "G1" });

            context.JobGradeGroupMaps.Add(new JobGradeGroupMap
            {
                JobGradeId = 1,
                GroupKey = "G1"
            });

            context.Positions.AddRange(
                new Position { PositionId = 1, JobGradeId = 1, OccupationalLevelId = 1 },
                new Position { PositionId = 2, JobGradeId = 1, OccupationalLevelId = 1 }
            );

      var employee = new Employee
      {
        EmployeeId = Guid.NewGuid().ToString(),
        PositionId = 1,
        Gender = Gender.Male,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
      };

      context.Employees.Add(employee);

            context.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Code = "AL",
                Name = "Annual Leave",
                Description = "Annual Leave",
                IsActive = true
            });
            context.LeaveEntitlementRules.Add(new LeaveEntitlementRule
            {
                Id = 1,
                LeaveTypeId = 1,
                GroupKey = "G1",
                MinYearsService = 0,
                MaxYearsService = null,
                DaysAllocated = 15,
                IsActive = true
            });

      await context.SaveChangesAsync();

            var service = CreateLeaveBalanceService(context);
            context.EmployeeAccrualRateHistories.Add(new EmployeeAccrualRateHistory
            {
                EmployeeId = employee.EmployeeId,
                AnnualEntitlement = 15,
                DailyRate = 15m / 260m,
                EffectiveFrom = employee.StartDate,
                EffectiveTo = null,
                CreatedDate = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

      await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            Assert.Single(context.EmployeeLeaveBalances);
        }

    [Fact]
    public async Task Initialize_ShouldNotDuplicate()
    {
      var context = GetDb();

            context.JobGrades.Add(new JobGrade { JobGradeId = 1, Name = "G1" });

            context.JobGradeGroupMaps.Add(new JobGradeGroupMap
            {
                JobGradeId = 1,
                GroupKey = "G1"
            });

            context.Positions.AddRange(
                new Position { PositionId = 1, JobGradeId = 1, OccupationalLevelId = 1 },
                new Position { PositionId = 2, JobGradeId = 1, OccupationalLevelId = 1 }
            );

      var employee = new Employee
      {
        EmployeeId = Guid.NewGuid().ToString(),
        PositionId = 1,
        Gender = Gender.Male,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
      };

      context.Employees.Add(employee);

            context.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Code = "AL",
                Name = "Annual Leave",
                Description = "Annual Leave",
                IsActive = true
            });
            context.LeaveEntitlementRules.Add(new LeaveEntitlementRule
            {
                Id = 1,
                LeaveTypeId = 1,
                GroupKey = "G1",
                MinYearsService = 0,
                MaxYearsService = null,
                DaysAllocated = 15,
                IsActive = true
            });

      await context.SaveChangesAsync();

      var service = CreateLeaveBalanceService(context);

            context.EmployeeAccrualRateHistories.Add(new EmployeeAccrualRateHistory
            {
                EmployeeId = employee.EmployeeId,
                AnnualEntitlement = 15,
                DailyRate = 15m / 260m,
                EffectiveFrom = employee.StartDate,
                EffectiveTo = null,
                CreatedDate = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            await service.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            Assert.Single(context.EmployeeLeaveBalances);
        }

    [Fact]
    public async Task Promotion_ShouldPreserveTakenDays()
    {
      var context = GetDb();

      context.JobGrades.AddRange(
          new JobGrade { JobGradeId = 1, Name = "G1" },
          new JobGrade { JobGradeId = 2, Name = "G2" });

            context.JobGradeGroupMaps.AddRange(
                new JobGradeGroupMap { JobGradeId = 1, GroupKey = "G1" },
                new JobGradeGroupMap { JobGradeId = 2, GroupKey = "G2" }
            );

            context.OccupationalLevels.Add(new OccupationalLevel { OccupationalLevelId = 1, Description = "Level 1" });

      context.Positions.AddRange(
          new Position { PositionId = 1, JobGradeId = 1, OccupationalLevelId = 1 },
          new Position { PositionId = 2, JobGradeId = 2, OccupationalLevelId = 1 });

      context.Users.Add(
        new User { UserId = 1, Email = "test@singular.co.za", PasswordHash = "dummy" }
       );
      var employee = new Employee
      {
        EmployeeId = Guid.NewGuid().ToString(),
        PositionId = 1,
        Gender = Gender.Male,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
        Email = "test@singular.co.za",
        Name = "Test",
        Surname = "User",
        ContactNumber = "0123456789",
        Nationality = "South African"
      };

      context.Employees.Add(employee);

            context.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Code = "AL",
                Name = "Annual Leave",
                Description = "Annual Leave",
                IsActive = true
            });
            context.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule
                {
                    Id = 1,
                    LeaveTypeId = 1,
                    GroupKey = "G1",
                    MinYearsService = 0,
                    MaxYearsService = null,
                    DaysAllocated = 15,
                    IsActive = true
                },
                new LeaveEntitlementRule
                {
                    Id = 2,
                    LeaveTypeId = 1,
                    GroupKey = "G2",
                    MinYearsService = 0,
                    MaxYearsService = null,
                    DaysAllocated = 20,
                    IsActive = true
                });

      await context.SaveChangesAsync();

            var balanceService = CreateLeaveBalanceService(context);
            var employeeService = CreateEmployeeService(context);
            context.EmployeeAccrualRateHistories.Add(new EmployeeAccrualRateHistory
            {
                EmployeeId = employee.EmployeeId,
                AnnualEntitlement = 15,
                DailyRate = 15m / 260m,
                EffectiveFrom = employee.StartDate,
                EffectiveTo = null,
                CreatedDate = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
            await balanceService.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

      var balance = context.EmployeeLeaveBalances.First();
      balance.TakenDays = 5;
      await context.SaveChangesAsync();

      await employeeService.UpdateEmployeeAsync(employee.EmployeeId, new UpdateEmployeeRequestDto
      {
        Title = Title.Mr,
        Gender = Gender.Male,
        Name = "Test",
        Surname = "User",
        Email = "test@singular.co.za",
        ContactNumber = "0123456789",
        City = "Johannesburg",
        ZipCode = "2000",
        IdNumber = "0305055400089",
        Nationality = "South African",
        Branch = Branch.Johannesburg,
        MonthlySalary = 10000,
        PositionId = 2,
        EmploymentStatus = EmploymentStatus.Permanent,
        ProfileImage = "img.jpg"
      });

            Assert.Equal(5, context.EmployeeLeaveBalances.First().TakenDays);
        }

    [Fact]
    public async Task Reset_ShouldCapCarryoverAtFive()
    {
      var context = GetDb();

            context.JobGrades.Add(new JobGrade { JobGradeId = 1, Name = "G1" });
            context.JobGradeGroupMaps.Add(new JobGradeGroupMap
            {
                JobGradeId = 1,
                GroupKey = "G1"
            });

            context.OccupationalLevels.Add(new OccupationalLevel { OccupationalLevelId = 1, Description = "Level 1" });

            context.Positions.AddRange(
                new Position { PositionId = 1, JobGradeId = 1, OccupationalLevelId = 1 },
                new Position { PositionId = 2, JobGradeId = 1, OccupationalLevelId = 1 }
            );

      var employee = new Employee
      {
        EmployeeId = Guid.NewGuid().ToString(),
        PositionId = 1,
        Gender = Gender.Male,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
      };

      context.Employees.Add(employee);

            context.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Code = "AL",
                Name = "Annual Leave",
                Description = "Annual Leave",
                IsActive = true
            });
            context.LeaveEntitlementRules.Add(new LeaveEntitlementRule
            {
                Id = 1,
                LeaveTypeId = 1,
                GroupKey = "G1",
                MinYearsService = 0,
                MaxYearsService = null,
                DaysAllocated = 15,
                IsActive = true
            });

      await context.SaveChangesAsync();

            var balanceService = CreateLeaveBalanceService(context);
            var processingService = CreateLeaveProcessingService(context);
            context.EmployeeAccrualRateHistories.Add(new EmployeeAccrualRateHistory
            {
                EmployeeId = employee.EmployeeId,
                AnnualEntitlement = 15,
                DailyRate = 15m / 260m,
                EffectiveFrom = employee.StartDate,
                EffectiveTo = null,
                CreatedDate = DateTime.UtcNow
            });
            
            await context.SaveChangesAsync();

      await balanceService.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

      var balance = context.EmployeeLeaveBalances.First();
      balance.AvailableDays = 12;

      await context.SaveChangesAsync();

      await processingService.ProcessAnnualResetAsync();

            Assert.Equal(5, context.EmployeeLeaveBalances.First().CarryoverDays);
        }

    [Fact]
    public async Task Initialize_ShouldThrowIfEmployeeNotFound()
    {
      var context = GetDb();
      var service = CreateLeaveBalanceService(context);

      await Assert.ThrowsAsync<InvalidOperationException>(() =>
          service.InitializeEmployeeLeaveBalancesAsync(Guid.NewGuid().ToString()));
    }
  }
}
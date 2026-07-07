namespace HRConnect.Tests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.Employee;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Services;
  using HRConnect.Api.Utils;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.AspNetCore.DataProtection;
  using Moq;
  using Xunit;

  public class EmployeeServiceBusinessTests
  {
    private sealed class FakeEmailService : IEmailService
    {
      public int EmailsSent { get; private set; }

      public Task SendEmailAsync(string recipientEmail, string subject, string body)
      {
        EmailsSent++;
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
      return new ApplicationDBContext(options, mockProtector.Object); ;
    }

    private static LeaveBalanceService GetBalanceService(ApplicationDBContext db)
        => new LeaveBalanceService(db);

    private static LeaveProcessingService GetProcessingService(ApplicationDBContext db)
        => new LeaveProcessingService(db, new FakeEmailService(), GetBalanceService(db));

        private static EmployeeService GetService(ApplicationDBContext db, FakeEmailService email)
        {
            var employeeRepoMock = new Mock<IEmployeeRepository>();
            var positionRepoMock = new Mock<IPositionRepository>();
            var companyRepoMock = new Mock<ICompanyRepository>();
            var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

            var activeCompanyService = new Mock<IActiveCompanyService>();
            var userCompanyService = new Mock<IUserCompanyService>();

      transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask);

      transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask);

      employeeRepoMock.Setup(x => x.BeginTransactionAsync())
          .ReturnsAsync(transactionMock.Object);



            // 🔥 FIX 1: RETURN DATA FROM DB
            employeeRepoMock.Setup(x => x.GetEmployeeByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => db.Employees.FirstOrDefault(e => e.EmployeeId == id));

      employeeRepoMock.Setup(x => x.UpdateEmployeeAsync(It.IsAny<Employee>()))
          .ReturnsAsync((Employee e) => e);

            employeeRepoMock.Setup(x => x.CreateEmployeeAsync(It.IsAny<Employee>()))
            .ReturnsAsync((Employee e) =>
            {
                db.Employees.Add(e);
                db.SaveChanges();
                return e;
            });

      employeeRepoMock.Setup(x => x.GetAllEmployeeIdsWithPrefix(It.IsAny<string>()))
          .ReturnsAsync(new List<string>());

            employeeRepoMock.Setup(x => x.GetEmployeeByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((Employee?)null);

      employeeRepoMock.Setup(x => x.GetEmployeeByTaxNumberAsync(It.IsAny<string>()))
          .ReturnsAsync((Employee?)null);

      employeeRepoMock.Setup(x => x.GetEmployeeByIdNumberAsync(It.IsAny<string>()))
          .ReturnsAsync((Employee?)null);

      employeeRepoMock.Setup(x => x.GetEmployeeByContactNumberAsync(It.IsAny<string>()))
          .ReturnsAsync((Employee?)null);

            positionRepoMock.Setup(x => x.GetPositionByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => db.Positions.FirstOrDefault(p => p.PositionId == id));
            if (!db.JobGradeGroupMaps.Any())
            {
                db.JobGradeGroupMaps.Add(new JobGradeGroupMap
                {
                    JobGradeId = 1,
                    GroupKey = "G1"
                });

                db.SaveChanges();
            }

            companyRepoMock
                .Setup(x => x.GetCompanyByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Company { CompanyId = "COMP001" });

            return new EmployeeService(
                db,
                activeCompanyService.Object,
                userCompanyService.Object,
                employeeRepoMock.Object,
                email,
                companyRepoMock.Object,
                positionRepoMock.Object,
                GetBalanceService(db),
                GetProcessingService(db),
                new PasswordHasher<User>()
            );
        }


    [Fact]
    public async Task CreateEmployee_ShouldInitializeLeaveBalances()
    {
      var db = GetDb();
      var email = new FakeEmailService();
      var service = GetService(db, email);

            db.JobGrades.Add(new JobGrade { JobGradeId = 1, Name = "G1" });

            db.JobGradeGroupMaps.Add(new JobGradeGroupMap
            {
                JobGradeId = 1,
                GroupKey = "G1"
            });

            db.OccupationalLevels.Add(new OccupationalLevel { OccupationalLevelId = 1, Description = "Level 1" });
            db.Positions.Add(new Position { PositionId = 1, JobGradeId = 1, OccupationalLevelId = 1 });

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
                GroupKey = "G1",
                DaysAllocated = 15,
                MinYearsService = 0,
                IsActive = true
            });

            await db.SaveChangesAsync();

            var result = await service.CreateEmployeeAsync(1, new CreateEmployeeRequestDto
            {
                Name = "Test",
                Surname = "User",
                Email = "test@singular.co.za",
                Title = Title.Mr,
                Gender = Gender.Male,
                ContactNumber = "0123456789",
                PhysicalAddress = "Address",
                TaxNumber = "1234567890",
                IdNumber = "0309195036087",
                Nationality = "South African",
                Branch = Branch.Johannesburg,
                City = "Johannesburg",
                ZipCode = "2000",
                PositionId = 1,
                MonthlySalary = 10000,
                EmploymentStatus = EmploymentStatus.Permanent,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                ProfileImage = "img.jpg"
            });

      Assert.Single(db.EmployeeLeaveBalances);
    }


    [Fact]
    public async Task UpdatePosition_ShouldCreateNewAccrualSegment()
    {
      var db = GetDb();
      var email = new FakeEmailService();
      var service = GetService(db, email);

      db.JobGrades.AddRange(
          new JobGrade { JobGradeId = 1, Name = "G1" },
          new JobGrade { JobGradeId = 2, Name = "G2" });

            db.JobGradeGroupMaps.AddRange(
                new JobGradeGroupMap { JobGradeId = 1, GroupKey = "G1" },
                new JobGradeGroupMap { JobGradeId = 2, GroupKey = "G2" }
            );
      db.OccupationalLevels.Add(new OccupationalLevel { OccupationalLevelId = 1, Description = "Level 1" });

      db.Positions.AddRange(
          new Position { PositionId = 1, JobGradeId = 1, OccupationalLevelId = 1 },
          new Position { PositionId = 2, JobGradeId = 2, OccupationalLevelId = 1 });

            db.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Code = "AL",
                Name = "Annual Leave",
                Description = "Annual Leave",
                IsActive = true
            });
            db.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule
                {
                    Id = 1,
                    LeaveTypeId = 1,
                    GroupKey = "G1",
                    DaysAllocated = 15,
                    IsActive = true
                },
                new LeaveEntitlementRule
                {
                    Id = 2,
                    LeaveTypeId = 1,
                    GroupKey = "G2",
                    DaysAllocated = 20,
                    IsActive = true
                });

      db.Users.Add(
              new User { UserId = 1, Email = "test@singular.co.za", PasswordHash = "dummy" }
             );
      var employee = new Employee
      {
        EmployeeId = Guid.NewGuid().ToString(),
        PositionId = 1,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
        Email = "test@singular.co.za",
        Gender = Gender.Male,
        Name = "Test",
        Surname = "User",
        ContactNumber = "0123456789",
        Nationality = "South African"
      };

      db.Employees.Add(employee);
      await db.SaveChangesAsync();

      await GetBalanceService(db).InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            await service.UpdateEmployeeAsync(1, employee.EmployeeId, new UpdateEmployeeRequestDto
            {
                Title = Title.Mr,
                Gender = Gender.Male,
                Name = "Test",
                Surname = "User",
                IdNumber = "0305054589589",
                Nationality = "South African",
                Email = "test@singular.co.za",
                ContactNumber = "0123456789",
                City = "Johannesburg",
                ZipCode = "2000",
                Branch = Branch.Johannesburg,
                MonthlySalary = 10000,
                PositionId = 2,
                EmploymentStatus = EmploymentStatus.Permanent,
                ProfileImage = "img.jpg"
            });

      Assert.Equal(2, db.EmployeeAccrualRateHistories.Count());
    }


    [Fact]
    public async Task UpdatePosition_ShouldThrowIfEmployeeNotFound()
    {
      var db = GetDb();
      var service = GetService(db, new FakeEmailService());

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.UpdateEmployeeAsync(1, "invalid", new UpdateEmployeeRequestDto
                {
                    Title = Title.Mr,
                    Gender = Gender.Male,
                    Name = "Test",
                    Surname = "User",
                    IdNumber = "0305054589589",
                    Nationality = "South African",
                    Email = "test@singular.co.za",
                    ContactNumber = "0123456789",
                    City = "Johannesburg",
                    ZipCode = "2000",
                    Branch = Branch.Johannesburg,
                    MonthlySalary = 10000,
                    PositionId = 1,
                    EmploymentStatus = EmploymentStatus.Permanent,
                    ProfileImage = "img.jpg"
                }));
        }
    }
}
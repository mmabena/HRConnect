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
  using Microsoft.EntityFrameworkCore.Storage;
  using Moq;
  using Xunit;

  public class EmployeeServiceTests : IDisposable
  {
    private readonly ApplicationDBContext _context;
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IPositionRepository> _positionRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILeaveBalanceService> _leaveBalanceServiceMock;
    private readonly Mock<ILeaveProcessingService> _leaveProcessingServiceMock;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly EmployeeService _employeeService;

    public EmployeeServiceTests()
    {
      var options = new DbContextOptionsBuilder<ApplicationDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

      _context = new ApplicationDBContext(options);
      _employeeRepositoryMock = new Mock<IEmployeeRepository>();
      _positionRepositoryMock = new Mock<IPositionRepository>();
      _emailServiceMock = new Mock<IEmailService>();
      _leaveBalanceServiceMock = new Mock<ILeaveBalanceService>();
      _leaveProcessingServiceMock = new Mock<ILeaveProcessingService>();
      _passwordHasherMock = new Mock<IPasswordHasher<User>>();

      _context.OccupationalLevels.Add(new OccupationalLevel
      {
        OccupationalLevelId = 1,
        Description = "Level"
      });

      _context.JobGrades.Add(new JobGrade
      {
        JobGradeId = 1,
        Name = "Grade"
      });

      _context.Positions.AddRange(
        new Position { PositionId = 1, JobGradeId = 1, OccupationalLevelId = 1, PositionTitle = "Developer" },
        new Position { PositionId = 2, JobGradeId = 1, OccupationalLevelId = 1, PositionTitle = "Manager" },
        new Position { PositionId = 4, JobGradeId = 1, OccupationalLevelId = 1, PositionTitle = "Analyst" },
        new Position { PositionId = 6, JobGradeId = 1, OccupationalLevelId = 1, PositionTitle = "Senior Developer" }
      );
      _context.SaveChanges();

      var transactionMock = new Mock<IDbContextTransaction>();
      transactionMock.Setup(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
      transactionMock.Setup(transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

      _employeeRepositoryMock.Setup(repository => repository.BeginTransactionAsync())
        .ReturnsAsync(transactionMock.Object);
      _employeeRepositoryMock.Setup(repository => repository.CreateEmployeeAsync(It.IsAny<Employee>()))
        .ReturnsAsync((Employee employee) =>
        {
          _context.Employees.Add(employee);
          _context.SaveChanges();
          return employee;
        });
      _employeeRepositoryMock.Setup(repository => repository.UpdateEmployeeAsync(It.IsAny<Employee>()))
        .ReturnsAsync((Employee employee) => employee);
      _employeeRepositoryMock.Setup(repository => repository.DeleteEmployeeAsync(It.IsAny<string>()))
        .ReturnsAsync(true);
      _employeeRepositoryMock.Setup(repository => repository.GetAllEmployeeIdsWithPrefix(It.IsAny<string>()))
        .ReturnsAsync(new List<string>());
      _employeeRepositoryMock.Setup(repository => repository.GetEmployeeByEmailAsync(It.IsAny<string>()))
        .ReturnsAsync((Employee?)null);
      _employeeRepositoryMock.Setup(repository => repository.GetEmployeeByTaxNumberAsync(It.IsAny<string>()))
        .ReturnsAsync((Employee?)null);
      _employeeRepositoryMock.Setup(repository => repository.GetEmployeeByIdNumberAsync(It.IsAny<string>()))
        .ReturnsAsync((Employee?)null);
      _employeeRepositoryMock.Setup(repository => repository.GetEmployeeByPassportAsync(It.IsAny<string>()))
        .ReturnsAsync((Employee?)null);
      _employeeRepositoryMock.Setup(repository => repository.GetEmployeeByContactNumberAsync(It.IsAny<string>()))
        .ReturnsAsync((Employee?)null);

      _positionRepositoryMock.Setup(repository => repository.GetPositionByIdAsync(It.IsAny<int>()))
        .ReturnsAsync((int id) => _context.Positions.FirstOrDefault(position => position.PositionId == id));

      _leaveBalanceServiceMock.Setup(service => service.InitializeEmployeeLeaveBalancesAsync(It.IsAny<string>()))
        .Returns(Task.CompletedTask);
      _leaveBalanceServiceMock.Setup(service => service.RecalculateAnnualLeaveAsync(It.IsAny<string>()))
        .Returns(Task.CompletedTask);
      _leaveProcessingServiceMock.Setup(service => service.RecalculateAllSickLeaveAsync())
        .Returns(Task.CompletedTask);
      _leaveProcessingServiceMock.Setup(service => service.RecalculateAllFamilyResponsibilityLeaveAsync())
        .Returns(Task.CompletedTask);
      _emailServiceMock.Setup(service => service.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .Returns(Task.CompletedTask);
      _passwordHasherMock.Setup(hasher => hasher.HashPassword(It.IsAny<User>(), It.IsAny<string>()))
        .Returns("hashed-password");

      _employeeService = new EmployeeService(
        _context,
        _employeeRepositoryMock.Object,
        _emailServiceMock.Object,
        _positionRepositoryMock.Object,
        _leaveBalanceServiceMock.Object,
        _leaveProcessingServiceMock.Object,
        _passwordHasherMock.Object);
    }

    [Fact]
    public async Task CreateEmployeeAsyncValidInputReturnsCreatedEmployee()
    {
      const string managerId = "MNG001";
      var request = CreateValidRequest();
      request.CareerManagerID = managerId;

      _employeeRepositoryMock
        .Setup(repository => repository.GetEmployeeByIdAsync(managerId))
        .ReturnsAsync(new Employee { EmployeeId = managerId, Email = "manager@singular.co.za" });

      var result = await _employeeService.CreateEmployeeAsync(request);

      Assert.NotNull(result);
      Assert.Equal(request.Name, result.Name);
      Assert.Contains(_context.Users, user => user.Email == request.Email && user.PasswordHash == "hashed-password");
    }

    [Fact]
    public async Task CreateEmployeeAsyncDuplicateEmailThrowsBusinessRuleException()
    {
      var request = CreateValidRequest();

      _employeeRepositoryMock
        .Setup(repository => repository.GetEmployeeByEmailAsync(request.Email))
        .ReturnsAsync(new Employee { EmployeeId = "EMP001", Email = request.Email });

      await Assert.ThrowsAsync<BusinessRuleException>(() => _employeeService.CreateEmployeeAsync(request));
    }

    [Fact]
    public async Task CreateEmployeeAsyncInvalidTitleGenderThrowsValidationException()
    {
      var request = CreateValidRequest();
      request.Title = Title.Mr;
      request.Gender = Gender.Female;

      _employeeRepositoryMock
        .Setup(repository => repository.GetEmployeeByEmailAsync(request.Email))
        .ReturnsAsync((Employee?)null);
      _employeeRepositoryMock
        .Setup(repository => repository.GetEmployeeByTaxNumberAsync(request.TaxNumber))
        .ReturnsAsync((Employee?)null);
      _employeeRepositoryMock
        .Setup(repository => repository.GetEmployeeByContactNumberAsync(request.ContactNumber))
        .ReturnsAsync((Employee?)null);

      await Assert.ThrowsAsync<ValidationException>(() => _employeeService.CreateEmployeeAsync(request));
    }

    [Fact]
    public async Task UpdateEmployeeAsyncValidInputReturnsUpdatedEmployeeAndSyncsUserEmail()
    {
      const string employeeId = "EMP001";
      const string managerId = "MNG002";
      const string existingEmail = "existing@singular.co.za";
      const string updatedEmail = "updated@singular.co.za";

      var existing = new Employee
      {
        EmployeeId = employeeId,
        Email = existingEmail,
        Name = "Existing",
        Surname = "User",
        PositionId = 1,
        Position = _context.Positions.First(position => position.PositionId == 1),
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
      };

      _context.Employees.Add(existing);
      _context.Users.Add(new User
      {
        UserId = 1,
        Email = existingEmail,
        PasswordHash = "hashed-password",
        Role = UserRole.NormalUser,
        CreatedAt = DateTime.UtcNow,
      });
      _context.LeaveTypes.Add(new LeaveType
      {
        Id = 1,
        Code = "AL",
        IsActive = true,
        Name = "Annual Leave",
        Description = "Annual Leave"
      });
      _context.LeaveEntitlementRules.Add(new LeaveEntitlementRule
      {
        LeaveTypeId = 1,
        JobGradeId = 1,
        MinYearsService = 0,
        MaxYearsService = null,
        DaysAllocated = 15,
        IsActive = true
      });
      _context.EmployeeAccrualRateHistories.Add(new EmployeeAccrualRateHistory
      {
        EmployeeId = employeeId,
        EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow),
        EffectiveTo = null
      });
      _context.SaveChanges();

      _employeeRepositoryMock
        .Setup(repository => repository.GetEmployeeByIdAsync(employeeId))
        .ReturnsAsync(existing);
      _employeeRepositoryMock
        .Setup(repository => repository.GetEmployeeByIdAsync(managerId))
        .ReturnsAsync(new Employee { EmployeeId = managerId, Email = "manager@singular.co.za" });

      var request = new UpdateEmployeeRequestDto
      {
        Name = "Updated",
        Surname = "User",
        Title = Title.Mr,
        Gender = Gender.Male,
        Email = updatedEmail,
        ContactNumber = "0987654321",
        IdNumber = "0305054589589",
        PhysicalAddress = "456 New Street",
        Nationality = "South African",
        City = "Johannesburg",
        ZipCode = "2000",
        Branch = Branch.Johannesburg,
        PositionId = 6,
        MonthlySalary = 30000,
        EmploymentStatus = EmploymentStatus.Permanent,
        CareerManagerID = managerId,
        ProfileImage = "updated.jpg",
        IsActive = true,
      };

      var result = await _employeeService.UpdateEmployeeAsync(employeeId, request);

      Assert.NotNull(result);
      Assert.Equal("Updated", result.Name);
      Assert.Contains(_context.Users, user => user.Email == updatedEmail);
    }

    [Fact]
    public async Task UpdateEmployeeAsyncEmployeeNotFoundThrowsNotFoundException()
    {
      _employeeRepositoryMock.Setup(repository => repository.GetEmployeeByIdAsync("X"))
        .ReturnsAsync((Employee?)null);

      await Assert.ThrowsAsync<NotFoundException>(() =>
        _employeeService.UpdateEmployeeAsync("X", new UpdateEmployeeRequestDto()));
    }

    [Fact]
    public async Task DeleteEmployeeAsyncValidIdReturnsTrue()
    {
      var employee = new Employee
      {
        EmployeeId = "EMP001",
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
      };

      _employeeRepositoryMock.Setup(repository => repository.GetEmployeeByIdAsync("EMP001"))
        .ReturnsAsync(employee);

      var result = await _employeeService.DeleteEmployeeAsync("EMP001");

      Assert.True(result);
    }

    [Fact]
    public async Task DeleteEmployeeAsyncEmployeeNotFoundThrowsNotFoundException()
    {
      _employeeRepositoryMock
        .Setup(repository => repository.GetEmployeeByIdAsync("EMP999"))
        .ReturnsAsync((Employee?)null);

      await Assert.ThrowsAsync<NotFoundException>(() => _employeeService.DeleteEmployeeAsync("EMP999"));
    }

    public void Dispose()
    {
      _context.Dispose();
      GC.SuppressFinalize(this);
    }

    private static CreateEmployeeRequestDto CreateValidRequest()
    {
      return new CreateEmployeeRequestDto
      {
        Title = Title.Ms,
        Name = "Jane",
        Surname = "Doe",
        PassportNumber = "A12345678",
        Nationality = "Botswanan",
        Gender = Gender.Female,
        ContactNumber = "0123456789",
        TaxNumber = "1234567890",
        Email = "jane.doe@singular.co.za",
        PhysicalAddress = "123 Main St",
        City = "Johannesburg",
        ZipCode = "2000",
        DateOfBirth = new DateOnly(1990, 1, 1),
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
        Branch = Branch.Johannesburg,
        MonthlySalary = 30000m,
        PositionId = 1,
        EmploymentStatus = EmploymentStatus.Permanent,
        ProfileImage = "profile.jpg",
        IsActive = true,
      };
    }
  }
}
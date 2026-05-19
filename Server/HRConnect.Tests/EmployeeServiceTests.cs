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
  using HRConnect.Api.DTOs.UserCompany;
  using HRConnect.Api.Utils;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Storage;
  using HRConnect.Api.Utils;
  using System.Linq;
  using Microsoft.AspNetCore.Identity;
  using System.Reflection.Metadata;
  using System.ComponentModel.DataAnnotations;
  using System.Runtime.Serialization;

  public class EmployeeServiceTests : IDisposable
  {
    private readonly ApplicationDBContext _context;
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IPositionRepository> _positionRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IActiveCompanyService> _activeCompanyServiceMock;
    private readonly Mock<IUserCompanyService> _userCompanyServiceMock;
    private readonly Mock<ILeaveBalanceService> _leaveBalanceServiceMock;
    private readonly Mock<ICompanyRepository> _companyRepoMock;
    private readonly Mock<ILeaveProcessingService> _leaveProcessingServiceMock;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly ApplicationDBContext _context;
    private readonly EmployeeService _employeeService;

    public EmployeeServiceTests()
    {
      _companyRepoMock = new Mock<ICompanyRepository>();
      _leaveBalanceServiceMock = new Mock<ILeaveBalanceService>();
      _leaveProcessingServiceMock = new Mock<ILeaveProcessingService>();
      _passwordHasherMock = new Mock<IPasswordHasher<User>>();

      var options = new DbContextOptionsBuilder<ApplicationDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

      _context = new ApplicationDBContext(options);
      _employeeRepositoryMock = new Mock<IEmployeeRepository>();
      _positionRepositoryMock = new Mock<IPositionRepository>();
      _activeCompanyServiceMock = new Mock<IActiveCompanyService>();
      _userCompanyServiceMock = new Mock<IUserCompanyService>();
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

      _companyRepoMock
          .Setup(x => x.GetCompanyByIdAsync(It.IsAny<string>()))
          .ReturnsAsync(new Company { CompanyId = "COMP001" });

      _activeCompanyServiceMock
          .Setup(x => x.GetActiveCompanyIdAsync(It.IsAny<int>()))
          .ReturnsAsync("COMP001");

      _userCompanyServiceMock
          .Setup(x => x.AssignCompanyToUserAsync(
              It.IsAny<int>(),
              It.IsAny<CreateUserCompanyDto>()))
          .Returns(Task.CompletedTask);

      // Position repo setup (dynamic)
      _positionRepositoryMock.Setup(p => p.GetPositionByIdAsync(It.IsAny<int>()))
          .ReturnsAsync((int id) =>
              _context.Positions.FirstOrDefault(p => p.PositionId == id));

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
          _activeCompanyServiceMock.Object,
          _userCompanyServiceMock.Object,
          _employeeRepositoryMock.Object,
          _emailServiceMock.Object,
          _companyRepoMock.Object,
          _positionRepositoryMock.Object,
          _leaveBalanceServiceMock.Object,
          _leaveProcessingServiceMock.Object,
          _passwordHasherMock.Object,
          _leaveProcessingServiceMock.Object,
          _passwordHasherMock.Object
      );
    }

    [Fact]
    public async Task CreateEmployeeAsyncValidInputReturnsCreatedEmployee()
    {
      //User and employee are tied by email
      var mockUser = new User
      {
        Email = "john.smith@singular.co.za",
        PasswordHash = "dummy_hash"
      };
      _passwordHasherMock.Setup(h => h.HashPassword(It.IsAny<User>(), It.IsAny<string>()))
                .Returns("hashedpassword");
      string managerId = "MNG001";
      var manager = new Employee { EmployeeId = managerId };

      var dto = new CreateEmployeeRequestDto
      {
        Name = "John",
        Surname = "Smith",
        Title = Title.Mr,
        Gender = Gender.Male,
        IdNumber = "0305055487589",
        TaxNumber = "1234567890",
        Nationality = "South African",
        PhysicalAddress = "123 Main St",
        Email = "john.smith@singular.co.za",
        ContactNumber = "0123456789",
        Branch = Branch.Johannesburg,
        City = "Johannesburg",
        ZipCode = "2000",
        PositionId = 4,
        MonthlySalary = 20000,
        EmploymentStatus = EmploymentStatus.Permanent,
        CareerManagerID = managerId,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        ProfileImage = "profile.jpg",
        PensionOptionId = 1,
      };

      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(managerId))
          .ReturnsAsync(manager);
      const string managerId = "MNG001";
      var request = CreateValidRequest();
      request.CareerManagerID = managerId;

      _employeeRepositoryMock
        .Setup(repository => repository.GetEmployeeByIdAsync(managerId))
        .ReturnsAsync(new Employee { EmployeeId = managerId, Email = "manager@singular.co.za" });

      var result = await _employeeService.CreateEmployeeAsync(1, request);

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

      await Assert.ThrowsAsync<BusinessRuleException>(() => _employeeService.CreateEmployeeAsync(1, request));
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

      await Assert.ThrowsAsync<ValidationException>(() =>
          _employeeService.CreateEmployeeAsync(dto));
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

      _context.Users.Add(new User
      {
        UserId = 1,
        Email = "test@singular.co.za",
        PasswordHash = "dummy"
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

      var result = await _employeeService.UpdateEmployeeAsync(1, employeeId, request);

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
        _employeeService.UpdateEmployeeAsync(1, "X", new UpdateEmployeeRequestDto()));
    }

    [Fact]
    public async Task DeleteEmployeeAsyncValidIdReturnsTrue()
    {
      var employee = new Employee
      {
        EmployeeId = "EMP001",
        CompanyId = "COMP001",
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
      };

      _employeeRepositoryMock.Setup(repository => repository.GetEmployeeByIdAsync("EMP001"))
        .ReturnsAsync(employee);

      var result = await _employeeService.DeleteEmployeeAsync(1, "EMP001");

      Assert.True(result);
    }

    [Fact]
    public async Task DeleteEmployeeAsyncEmployeeNotFoundThrowsNotFoundException()
    {
      _employeeRepositoryMock
        .Setup(repository => repository.GetEmployeeByIdAsync("EMP999"))
        .ReturnsAsync((Employee?)null);

      await Assert.ThrowsAsync<NotFoundException>(() => _employeeService.DeleteEmployeeAsync(1, "EMP999"));
    }

    [Fact]
    public async Task GetAllEmployeesAsync_UsesActiveCompanyId()
    {
      var userId = 1;

      _activeCompanyServiceMock
        .Setup(x => x.GetActiveCompanyIdAsync(userId))
        .ReturnsAsync("COMP001");

      _employeeRepositoryMock
        .Setup(x => x.GetAllEmployeeByCompanyAsync("COMP001"))
        .ReturnsAsync(new List<Employee>
        {
          new Employee { EmployeeId = "EMP001", CompanyId = "COMP001" },
          new Employee { EmployeeId = "EMP002", CompanyId = "COMP001" }
        });

      var result = await _employeeService.GetAllEmployeesAsync(userId);

      Assert.NotNull(result);
      Assert.Equal("COMP001", result.First().CompanyId);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_DifferentCompany_ThrowUnauthrizedAccessException()
    {
      var userId = 1;
      var employeeId = "EMP001";

      _activeCompanyServiceMock
        .Setup(x => x.GetActiveCompanyIdAsync(userId))
        .ReturnsAsync("COMP001");

      _employeeRepositoryMock
        .Setup(x => x.GetEmployeeByIdAsync(employeeId))
        .ReturnsAsync(new Employee { EmployeeId = employeeId, CompanyId = "COMP002" });

      await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
        _employeeService.GetEmployeeByIdAsync(userId, employeeId));
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
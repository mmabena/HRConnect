namespace HRConnect.Tests
{
  using System;
  using HRConnect.Api.DTOs.Company;
  using HRConnect.Api.DTOs.UserCompany;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.Linq;
  using System.Reflection.Metadata;
  using System.Runtime.Serialization;
  using System.Threading;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.Employee;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Services;
  using HRConnect.Api.Utils;
  using System.Linq;
  using Microsoft.AspNetCore.SignalR;
  using HRConnect.Api.Hubs;
  using Microsoft.AspNetCore.SignalR;
  using HRConnect.Api.Hubs;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Storage;
  using Moq;
  using Xunit;

  public class EmployeeServiceTests : IDisposable
  {
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IPositionRepository> _positionRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILeaveBalanceService> _leaveBalanceServiceMock;
    private readonly Mock<ILeaveProcessingService> _leaveProcessingServiceMock;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly ApplicationDBContext _context;
    private readonly Mock<IActiveCompanyService> _activeCompanyServiceMock;
    private readonly Mock<IUserCompanyService> _userCompanyServiceMock;
    private readonly Mock<ICompanyRepository> _companyRepoMock;
    private readonly EmployeeService _employeeService;

 



    public EmployeeServiceTests()
    {
      _employeeRepoMock = new Mock<IEmployeeRepository>();
      _emailServiceMock = new Mock<IEmailService>();
      _positionRepoMock = new Mock<IPositionRepository>();
      _activeCompanyServiceMock = new Mock<IActiveCompanyService>();
      _userCompanyServiceMock = new Mock<IUserCompanyService>();
      _companyRepoMock = new Mock<ICompanyRepository>();
      _leaveBalanceServiceMock = new Mock<ILeaveBalanceService>();
      _leaveProcessingServiceMock = new Mock<ILeaveProcessingService>();
     
      _passwordHasherMock = new Mock<IPasswordHasher<User>>();

      var options = new DbContextOptionsBuilder<ApplicationDBContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;

      _context = new ApplicationDBContext(options);

      _context.OccupationalLevels.Add(new OccupationalLevel
      {
        OccupationalLevelId = 1,
        Description = "Level"
      });

      _activeCompanyServiceMock
    .Setup(x => x.GetActiveCompanyIdAsync(It.IsAny<int>()))
    .ReturnsAsync("COMP001");

      _companyRepoMock
          .Setup(x => x.GetCompanyByIdAsync(It.IsAny<string>()))
          .ReturnsAsync(new Company
          {
            CompanyId = "COMP001",
            CompanyName = "Test Company"
          });

      _userCompanyServiceMock
          .Setup(x => x.AssignCompanyToUserAsync(
              It.IsAny<int>(),
              It.IsAny<CreateUserCompanyDto>()))
          .Returns(Task.CompletedTask);

      _context.JobGrades.Add(new JobGrade
      {
        JobGradeId = 1,
        Name = "Grade"
      });
      _context.JobGradeGroupMaps.Add(new JobGradeGroupMap
      {
        JobGradeId = 1,
        GroupKey = "G1"
      });

      _context.Positions.AddRange(
          new Position { PositionId = 1, JobGradeId = 1, OccupationalLevelId = 1 },
          new Position { PositionId = 2, JobGradeId = 1, OccupationalLevelId = 1 },
          new Position { PositionId = 4, JobGradeId = 1, OccupationalLevelId = 1 }
      );
      _context.Positions.Add(
        new Position
        {
          PositionId = 6,
          JobGradeId = 1,
          OccupationalLevelId = 1,
          PositionTitle = "Senior Developer"
        }
    );

      _context.SaveChanges();

      var transactionMock = new Mock<IDbContextTransaction>();
      transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask);
      transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask);

      _employeeRepoMock.Setup(r => r.BeginTransactionAsync())
          .ReturnsAsync(transactionMock.Object);

      _employeeRepoMock.Setup(x => x.CreateEmployeeAsync(It.IsAny<Employee>()))
          .ReturnsAsync((Employee e) =>
          {
            _context.Employees.Add(e);
            _context.SaveChanges();
            return e;
          });

      _employeeRepoMock.Setup(x => x.UpdateEmployeeAsync(It.IsAny<Employee>()))
          .ReturnsAsync((Employee e) => e);

      _employeeRepoMock.Setup(x => x.DeleteEmployeeAsync(It.IsAny<string>()))
          .ReturnsAsync(true);

      _employeeRepoMock.Setup(x => x.GetAllEmployeeIdsWithPrefix(It.IsAny<string>()))
          .ReturnsAsync(new List<string>());

      _employeeRepoMock.Setup(x => x.GetEmployeeByEmailAsync(It.IsAny<string>()))
          .ReturnsAsync((Employee?)null);

      _employeeRepoMock.Setup(x => x.GetEmployeeByTaxNumberAsync(It.IsAny<string>()))
          .ReturnsAsync((Employee?)null);

      _employeeRepoMock.Setup(x => x.GetEmployeeByIdNumberAsync(It.IsAny<string>()))
          .ReturnsAsync((Employee?)null);

      _employeeRepoMock.Setup(x => x.GetEmployeeByPassportAsync(It.IsAny<string>()))
          .ReturnsAsync((Employee?)null);

      _employeeRepoMock.Setup(x => x.GetEmployeeByContactNumberAsync(It.IsAny<string>()))
          .ReturnsAsync((Employee?)null);

      _positionRepoMock.Setup(p => p.GetPositionByIdAsync(It.IsAny<int>()))
          .ReturnsAsync((int id) =>
              _context.Positions.FirstOrDefault(p => p.PositionId == id));

      _leaveBalanceServiceMock.Setup(x => x.InitializeEmployeeLeaveBalancesAsync(It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
          .Returns(Task.CompletedTask);

      _employeeService = new EmployeeService(
          _context,
          _employeeRepoMock.Object,
          _emailServiceMock.Object,
          _positionRepoMock.Object,
          _leaveBalanceServiceMock.Object,
          _leaveProcessingServiceMock.Object,
       

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

      var employeeRequestDto = new CreateEmployeeRequestDto
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

      var result = await _employeeService.CreateEmployeeAsync(1, employeeRequestDto);

      Assert.NotNull(result);
      Assert.Equal("John", result.Name);
    }

    [Fact]
    public async Task CreateEmployeeAsyncDuplicateEmailThrowsBusinessRuleException()
    {
      var dto = new CreateEmployeeRequestDto
      {
        Name = "Jane",
        Surname = "Doe",
        Title = Title.Ms,
        Gender = Gender.Female,
        Email = "duplicate@singular.co.za",
        ContactNumber = "0123456789",
        IdNumber = "0305054589589",
        TaxNumber = "1234567890",
        Nationality = "South African",
        PhysicalAddress = "123 Main St",
        Branch = Branch.Johannesburg,
        City = "Johannesburg",
        ZipCode = "2000",
        PositionId = 4,
        MonthlySalary = 20000,
        EmploymentStatus = EmploymentStatus.Permanent,
        CareerManagerID = "MNG001",
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        ProfileImage = "profile.jpg"
      };

      _employeeRepoMock.Setup(r => r.GetEmployeeByEmailAsync(dto.Email))
          .ReturnsAsync(new Employee { Email = dto.Email });

      await Assert.ThrowsAsync<BusinessRuleException>(() =>
          _employeeService.CreateEmployeeAsync(1, dto));
    }

    [Fact]
    public async Task CreateEmployeeAsyncInvalidTitleGenderThrowsValidationException()
    {
      var dto = new CreateEmployeeRequestDto
      {
        Name = "Alex",
        Surname = "King",
        Title = Title.Mr,
        Gender = Gender.Female,
        Email = "alex@singular.co.za",
        ContactNumber = "0123456789",
        IdNumber = "0305054589589",
        TaxNumber = "1234567890",
        PositionId = 4,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
      };

      await Assert.ThrowsAsync<HRConnect.Api.Services.ValidationException>(() =>
          _employeeService.CreateEmployeeAsync(1, dto));
    }

    [Fact]
    public async Task UpdateEmployeeAsyncValidInputReturnsUpdatedEmployee()
    {
      var employeeId = "EMP001";
      var managerId = "MNG002";

      var existing = new Employee
      {
        EmployeeId = employeeId,
        Email = "test@singular.co.za",
        PositionId = 1,
        Position = _context.Positions.First(p => p.PositionId == 1),
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
      };
      _context.Employees.Add(existing);
      _context.LeaveTypes.Add(new LeaveType
      {
        Id = 1,
        Code = "AL",
        IsActive = true,
        Name = "Annual Leave",
        Description = "Annual Leave"
      });
      _context.JobGradeGroupMaps.Add(new JobGradeGroupMap
      {
        JobGradeId = 1,
        GroupKey = "G1"
      });
      _context.LeaveEntitlementRules.Add(new LeaveEntitlementRule
      {
        LeaveTypeId = 1,
        GroupKey = "G1",
        MinYearsService = 0,
        MaxYearsService = null,
        DaysAllocated = 15,
        IsActive = true
      });
      _context.EmployeeAccrualRateHistories.Add(new EmployeeAccrualRateHistory
      {
        EmployeeId = "EMP001",
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
      var manager = new Employee { EmployeeId = managerId };

      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employeeId))
          .ReturnsAsync(existing);

      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(managerId))
          .ReturnsAsync(manager);

      var dto = new UpdateEmployeeRequestDto
      {
        Name = "Updated",
        Surname = "User",
        Title = Title.Mr,
        Gender = Gender.Male,
        Email = "updated@singular.co.za",
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
        ProfileImage = "updated.jpg"
      };

      var result = await _employeeService.UpdateEmployeeAsync(1, employeeId, dto);

      Assert.NotNull(result);
      Assert.Equal("Updated", result.Name);
    }

    [Fact]
    public async Task UpdateEmployeeAsyncEmployeeNotFoundThrowsNotFoundException()
    {
      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync("X"))
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
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
      };

      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync("EMP001"))
          .ReturnsAsync(employee);

      var result = await _employeeService.DeleteEmployeeAsync(1, "EMP001");

      Assert.True(result);
    }

    [Fact]
    public async Task DeleteEmployeeAsyncEmployeeNotFoundThrowsNotFoundException()
    {
      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync("X"))
          .ReturnsAsync((Employee?)null);

      await Assert.ThrowsAsync<NotFoundException>(() =>
          _employeeService.DeleteEmployeeAsync(1, "X"));
    }

    public void Dispose()
    {
      _context.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}
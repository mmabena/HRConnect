namespace HRConnect.Tests
{
  using Xunit;
  using Moq;
  using HRConnect.Api.Services;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Employee;
  using HRConnect.Api.Utils;
  using HRConnect.Api.Mappers;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using HRConnect.Api.Data;

  public class EmployeeServiceTests : IDisposable
  {
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IPositionRepository> _positionRepoMock;
    private readonly Mock<ILeaveBalanceService> _leaveBalanceServiceMock;
    private readonly Mock<ILeaveProcessingService> _leaveProcessingServiceMock;
    private readonly ApplicationDBContext _context;

    private readonly EmployeeService _employeeService;

    public EmployeeServiceTests()
    {
      _employeeRepoMock = new Mock<IEmployeeRepository>();
      _emailServiceMock = new Mock<IEmailService>();
      _positionRepoMock = new Mock<IPositionRepository>();
      _leaveBalanceServiceMock = new Mock<ILeaveBalanceService>();
      _leaveProcessingServiceMock = new Mock<ILeaveProcessingService>();

      var options = new DbContextOptionsBuilder<ApplicationDBContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString()) // FIX: no duplicate DB
          .Options;

      _context = new ApplicationDBContext(options);

      // ✅ MINIMAL REQUIRED SEED DATA
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
        new Position
        {
          PositionId = 1,
          PositionTitle = "Position 1",
          JobGradeId = 1,
          OccupationalLevelId = 1
        },
        new Position
        {
          PositionId = 2,
          PositionTitle = "Position 2",
          JobGradeId = 1,
          OccupationalLevelId = 1
        }
      );

      _context.LeaveTypes.Add(new LeaveType
      {
        Id = 1,
        Code = "AL",
        Name = "Annual Leave",
        Description = "Annual Leave",
        IsActive = true
      });

      _context.LeaveEntitlementRules.Add(new LeaveEntitlementRule
      {
        Id = 1,
        LeaveTypeId = 1,
        JobGradeId = 1,
        DaysAllocated = 15,
        MinYearsService = 0,
        MaxYearsService = null,
        IsActive = true
      });

      _context.SaveChanges();

      _employeeService = new EmployeeService(
          _context,
          _employeeRepoMock.Object,
          _emailServiceMock.Object,
          _positionRepoMock.Object,
          _leaveBalanceServiceMock.Object,
          _leaveProcessingServiceMock.Object
      );
    }

    [Fact]
    public async Task CreateEmployeeAsyncValidInputReturnsCreatedEmployee()
    {
      string managerId = "MNG001";
      var manager = new Employee { EmployeeId = managerId };

      var employeeDto = new CreateEmployeeRequestDto
      {
        Name = "John",
        Surname = "Smith",
        Title = Title.Mr,
        Gender = Gender.Male,
        IdNumber = "0305055487589",
        TaxNumber = "1234567890",
        PhysicalAddress = "123 Main St",
        Email = "john.smith@singular.co.za",
        ContactNumber = "0123456789",
        Branch = Branch.Johannesburg,
        City = "Johannesburg",
        ZipCode = "2000",
        Nationality = "South African", // REQUIRED
        PositionId = 1,
        MonthlySalary = 20000,
        EmploymentStatus = EmploymentStatus.Permanent,
        CareerManagerID = managerId,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        ProfileImage = "profile.jpg"
      };

      var createdEmployee = employeeDto.ToEmployeeFromCreateDTO();

      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(managerId))
                       .ReturnsAsync(manager);

      _employeeRepoMock.Setup(r => r.CreateEmployeeAsync(It.IsAny<Employee>()))
                       .ReturnsAsync(createdEmployee);

      _employeeRepoMock.Setup(r => r.GetAllEmployeeIdsWithPrefix(It.IsAny<string>()))
                       .ReturnsAsync(new List<string>());

      _employeeRepoMock.Setup(r => r.GetEmployeeByEmailAsync(It.IsAny<string>()))
                       .ReturnsAsync((Employee?)null);

      _positionRepoMock.Setup(p => p.GetPositionByIdAsync(It.IsAny<int>()))
                       .ReturnsAsync(new Position { PositionId = 1, JobGradeId = 1, OccupationalLevelId = 1 });

      _leaveBalanceServiceMock.Setup(x => x.InitializeEmployeeLeaveBalancesAsync(It.IsAny<string>()))
                              .Returns(Task.CompletedTask);

      var tx = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
      _employeeRepoMock.Setup(r => r.BeginTransactionAsync())
                       .ReturnsAsync(tx.Object);

      _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                       .Returns(Task.CompletedTask);

      var result = await _employeeService.CreateEmployeeAsync(employeeDto);

      Assert.NotNull(result);
      Assert.Equal("John", result.Name);
      Assert.Equal("Smith", result.Surname);
    }

    [Fact]
    public async Task CreateEmployeeAsyncDuplicateEmailThrowsBusinessRuleException()
    {
      var dto = new CreateEmployeeRequestDto
      {
        Name = "Jane",
        Surname = "Doe",
        Email = "jane.doe@singular.co.za",
        ContactNumber = "0123456789",
        IdNumber = "0305054589589",
        PhysicalAddress = "123 Main St",
        TaxNumber = "1234567890",
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        Branch = Branch.Johannesburg,
        City = "Johannesburg",
        ZipCode = "2000",
        Nationality = "South African",
        Title = Title.Ms,
        Gender = Gender.Female,
        PositionId = 1,
        MonthlySalary = 30000,
        EmploymentStatus = EmploymentStatus.Permanent,
        ProfileImage = "profile.jpg"
      };

      _employeeRepoMock.Setup(r => r.GetEmployeeByEmailAsync(dto.Email))
                       .ReturnsAsync(new Employee());

      await Assert.ThrowsAsync<BusinessRuleException>(() =>
          _employeeService.CreateEmployeeAsync(dto));
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
        Email = "alex.king@singular.co.za",
        ContactNumber = "0123456789",
        PhysicalAddress = "123 Main St",
        IdNumber = "0305054589589",
        TaxNumber = "1234567890",
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        Branch = Branch.Johannesburg,
        City = "Johannesburg",
        ZipCode = "2000",
        Nationality = "South African",
        PositionId = 1,
        MonthlySalary = 20000,
        EmploymentStatus = EmploymentStatus.Permanent,
        ProfileImage = "profile.jpg"
      };

      await Assert.ThrowsAsync<HRConnect.Api.Services.ValidationException>(() =>
          _employeeService.CreateEmployeeAsync(dto));
    }

    [Fact]
    public async Task UpdateEmployeeAsyncValidInputReturnsUpdatedEmployee()
    {
      var employeeId = "EMP001";

      var existingEmployee = new Employee
      {
        EmployeeId = employeeId,
        Name = "OldName",
        Surname = "OldSurname",
        Email = "old@singular.co.za",
        PositionId = 1,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
        Gender = Gender.Male
      };

      _context.Employees.Add(existingEmployee);
      await _context.SaveChangesAsync();

      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employeeId))
                       .ReturnsAsync(existingEmployee);

      _employeeRepoMock.Setup(r => r.UpdateEmployeeAsync(It.IsAny<Employee>()))
                       .ReturnsAsync((Employee e) => e);

      _positionRepoMock.Setup(p => p.GetPositionByIdAsync(2))
                       .ReturnsAsync(new Position { PositionId = 2, JobGradeId = 1, OccupationalLevelId = 1 });

      var updateDto = new UpdateEmployeeRequestDto
      {
        Title = Title.Mr,
        Name = "UpdatedName",
        Surname = "UpdatedSurname",
        IdNumber = "0305054589589",
        Email = "updated@singular.co.za",
        ContactNumber = "0123456789",
        City = "Johannesburg",
        ZipCode = "2000",
        Gender = Gender.Male,
        Nationality = "South African",
        Branch = Branch.Johannesburg,
        MonthlySalary = 35000,
        PositionId = 2,
        EmploymentStatus = EmploymentStatus.Permanent,
        ProfileImage = "updated.jpg"
      };

      var result = await _employeeService.UpdateEmployeeAsync(employeeId, updateDto);

      Assert.NotNull(result);
      Assert.Equal("UpdatedName", result.Name);
      Assert.Equal("UpdatedSurname", result.Surname);
    }

    [Fact]
    public async Task DeleteEmployeeAsyncValidIdReturnsTrue()
    {
      var employeeId = "EMP001";

      var employee = new Employee
      {
        EmployeeId = employeeId,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
      };

      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employeeId))
                       .ReturnsAsync(employee);

      _employeeRepoMock.Setup(r => r.DeleteEmployeeAsync(employeeId))
                       .ReturnsAsync(true);

      var result = await _employeeService.DeleteEmployeeAsync(employeeId);

      Assert.True(result);
    }

    [Fact]
    public async Task DeleteEmployeeAsyncEmployeeNotFoundThrowsNotFoundException()
    {
      var employeeId = "EMP404";

      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employeeId))
                       .ReturnsAsync((Employee?)null);

      await Assert.ThrowsAsync<NotFoundException>(() =>
          _employeeService.DeleteEmployeeAsync(employeeId));
    }
    [Fact]
    public void Dispose()
    {
      _context.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}
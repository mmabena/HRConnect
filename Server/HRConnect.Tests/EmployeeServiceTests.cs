namespace HRConnect.Tests
{
  using Xunit;
  using Moq;
  using HRConnect.Api.Services;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Employee;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using HRConnect.Api.Data;
  using System.Threading;
  using Microsoft.EntityFrameworkCore.Storage;
  using HRConnect.Api.Utils;
  public class EmployeeServiceTests : IDisposable
  {
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IPositionRepository> _positionRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILeaveBalanceService> _leaveBalanceServiceMock;
    private readonly Mock<ILeaveProcessingService> _leaveProcessingServiceMock;

    private readonly ApplicationDBContext _context;
    private readonly EmployeeService _employeeService;

    public EmployeeServiceTests()
    {
      _employeeRepoMock = new Mock<IEmployeeRepository>();
      _emailServiceMock = new Mock<IEmailService>();
      _positionRepoMock = new Mock<IPositionRepository>();

      _employeeService = new EmployeeService(
          _employeeRepoMock.Object,
          _emailServiceMock.Object,
          _positionRepoMock.Object
      );
    }


    [Fact]
    public async Task CreateEmployeeAsyncValidInputReturnsCreatedEmployee()
    {
      // Arrange
      string managerId = "MNG001";

      _context = new ApplicationDBContext(options);

      // ✅ Seed
      _context.OccupationalLevels.Add(new OccupationalLevel
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
        ProfileImage = "profile.jpg"
      };

      _context.JobGrades.Add(new JobGrade
      {
        JobGradeId = 1,
        Name = "Grade"
      });

      _context.Positions.AddRange(
        new Position { PositionId = 1, JobGradeId = 1, OccupationalLevelId = 1 },
        new Position { PositionId = 2, JobGradeId = 1, OccupationalLevelId = 1 }
      );

      _context.SaveChanges();

      // ✅ Transaction mock
      var transactionMock = new Mock<IDbContextTransaction>();
      transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask);
      transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask);

      _positionRepoMock.Setup(p => p.GetPositionByIdAsync(4))
    .ReturnsAsync(new Position
        {
          PositionId = 4,
          PositionTitle = "Developer"
        });

      var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
      _employeeRepoMock.Setup(r => r.BeginTransactionAsync())
          .ReturnsAsync(transactionMock.Object);

      // ✅ Required repo setups
      _employeeRepoMock.Setup(x => x.CreateEmployeeAsync(It.IsAny<Employee>()))
          .ReturnsAsync((Employee e) =>
          {
            _context.Employees.Add(e);
            _context.SaveChanges();
            return e;
          });

      _employeeRepoMock.Setup(x => x.GetAllEmployeeIdsWithPrefix(It.IsAny<string>()))
          .ReturnsAsync(new List<string>());

      _employeeRepoMock.Setup(x => x.GetEmployeeByEmailAsync(It.IsAny<string>()))
          .ReturnsAsync((Employee?)null);

      _employeeRepoMock.Setup(x => x.GetEmployeeByTaxNumberAsync(It.IsAny<string>()))
          .ReturnsAsync((Employee?)null);

    [Fact]
    public async Task CreateEmployeeAsyncDuplicateEmailThrowsBusinessRuleException()
    {
      var employeeDto = new CreateEmployeeRequestDto
      {
        Name = "Jane",
        Surname = "Doe",
        Email = "jane.doe@singular.co.za",
        ContactNumber = "0123456789",
        IdNumber = "0305054589589",
        PhysicalAddress = "123 Main St",
        TaxNumber = "1234567890",
        Nationality = "South African",
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        Branch = Branch.Johannesburg,
        City = "Johannesburg",
        ZipCode = "2000",
        Title = Title.Ms,
        Gender = Gender.Female,
        PositionId = 4,
        MonthlySalary = 30000,
        EmploymentStatus = EmploymentStatus.Permanent,
        CareerManagerID = "Manager Name",
        ProfileImage = "profile.jpg"
      };

      _employeeRepoMock.Setup(x => x.GetEmployeeByContactNumberAsync(It.IsAny<string>()))
          .ReturnsAsync((Employee?)null);

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

      var dto = new CreateEmployeeRequestDto
      {
        Name = "John",
        Surname = "Smith",
        Title = Title.Mr,
        Gender = Gender.Female, // invalid
        Email = "alex.king@singular.co.za",
        ContactNumber = "0123456789",
        PhysicalAddress = "123 Main St",
        IdNumber = "0305054589589",
        Nationality = "Zimbabwean",
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
        ProfileImage = "profile.jpg"
      };

      await Assert.ThrowsAsync<HRConnect.Api.Services.ValidationException>(() =>
          _employeeService.CreateEmployeeAsync(employeeDto));
    }

    [Fact]
    public async Task UpdateEmployeeAsyncValidInputReturnsUpdatedEmployee()
    {
      // Arrange
      var employeeId = "EMP001";
      string managerId = "New Manager";
      var newManager = new Employee { EmployeeId = managerId };

      var updateDto = new UpdateEmployeeRequestDto
      {
        Title = Title.Mr,
        Name = "UpdatedName",
        Surname = "UpdatedSurname",
        IdNumber = "0305054589589",
        Gender = Gender.Male,
        PassportNumber = "",
        ContactNumber = "0987654321",
        Email = "updated@singular.co.za",
        PhysicalAddress = "456 New Street",
        Nationality = "South African",
        City = "Cape Town",
        ZipCode = "8000",
        HasDisability = false,
        DisabilityDescription = null,
        Branch = Branch.Johannesburg,
        MonthlySalary = 35000,
        PositionId = 6,
        EmploymentStatus = EmploymentStatus.Permanent,
        CareerManagerID = newManager.EmployeeId,
        ProfileImage = "updated.jpg"
      };

      var existingEmployee = new Employee
      {
        EmployeeId = employeeId,
        Name = "OldName",
        Surname = "OldSurname",
        Email = "old@email.com",
        IdNumber = "0305054589589"
      };

      //creating a new Manager to assign employee to 
      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(managerId))
          .ReturnsAsync(manager);

      _positionRepoMock.Setup(p => p.GetPositionByIdAsync(1))
    .ReturnsAsync(() => _context.Positions.First(p => p.PositionId == 1));

      _employeeRepoMock.Setup(r => r.UpdateEmployeeAsync(It.IsAny<Employee>()))
                       .ReturnsAsync((Employee e) => e);
      
      _positionRepoMock.Setup(p => p.GetPositionByIdAsync(6))
            .ReturnsAsync(new Position
            {
                PositionId = 6,
                PositionTitle = "Senior Developer"
            });

      var result = await _employeeService.CreateEmployeeAsync(dto);

      Assert.NotNull(result);
      Assert.Equal("John", result.Name);
    }

    [Fact]
    public async Task DeleteEmployeeAsyncEmployeeNotFoundThrowsNotFoundException()
    {
      var employeeId = "EMP999";
      var updateDto = new UpdateEmployeeRequestDto
      {
        Title = Title.Mr,
        Name = "Test",
        Surname = "User",
        IdNumber = "0305054589589",
        ContactNumber = "0123456789",
        Email = "test@singular.co.za",
        Gender = Gender.Male,
        PhysicalAddress = "123 Street",
        City = "Johannesburg",
        Nationality = "South African",
        ZipCode = "2000",
        Branch = Branch.Johannesburg,
        MonthlySalary = 20000,
        PositionId = 5,
        EmploymentStatus = EmploymentStatus.Permanent,
        CareerManagerID = "Manager",
        ProfileImage = "profile.jpg"
      };

      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employeeId))
                       .ReturnsAsync((Employee?)null);

      await Assert.ThrowsAsync<NotFoundException>(() =>
          _employeeService.DeleteEmployeeAsync("X"));
    }

    public void Dispose()
    {
      _context.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}
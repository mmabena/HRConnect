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
  using System.Net.NetworkInformation;
  using System.Runtime.CompilerServices;

  public class EmployeeServiceTests
  {
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly EmployeeService _employeeService;


    public EmployeeServiceTests()
    {
      _employeeRepoMock = new Mock<IEmployeeRepository>();
      _emailServiceMock = new Mock<IEmailService>();

      _employeeService = new EmployeeService(
          _employeeRepoMock.Object,
          _emailServiceMock.Object
      );
    }


    [Fact]
    public async Task CreateEmployeeAsyncValidInputReturnsCreatedEmployee()
    {
      // Arrange
      string managerId = "MNG001";

      var manager = new Employee { EmployeeId = managerId, };

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
        PositionId = 1,
        MonthlySalary = 20000,
        EmploymentStatus = EmploymentStatus.Permanent,
        CareerManagerID = managerId,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        ProfileImage = "profile.jpg"
      };

      var createdEmployee = employeeDto.ToEmployeeFromCreateDTO();
      //Creating the manager 
      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(managerId))
                       .ReturnsAsync(manager);

      _employeeRepoMock.Setup(r => r.CreateEmployeeAsync(It.IsAny<Employee>()))
                       .ReturnsAsync(createdEmployee);

      _employeeRepoMock.Setup(r => r.GetAllEmployeeIdsWithPrefix(It.IsAny<string>()))
                       .ReturnsAsync(new List<string>());

      _employeeRepoMock.Setup(r => r.GetEmployeeByEmailAsync(It.IsAny<string>()))
                       .ReturnsAsync((Employee?)null);
      _employeeRepoMock.Setup(r => r.GetEmployeeByTaxNumberAsync(It.IsAny<string>()))
                       .ReturnsAsync((Employee?)null);
      _employeeRepoMock.Setup(r => r.GetEmployeeByIdNumberAsync(It.IsAny<string>()))
                       .ReturnsAsync((Employee?)null);
      _employeeRepoMock.Setup(r => r.GetEmployeeByPassportAsync(It.IsAny<string>()))
                       .ReturnsAsync((Employee?)null);
      _employeeRepoMock.Setup(r => r.GetEmployeeByContactNumberAsync(It.IsAny<string>()))
                       .ReturnsAsync((Employee?)null);

      var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
      _employeeRepoMock.Setup(r => r.BeginTransactionAsync())
                       .ReturnsAsync(mockTransaction.Object); // <-- Return null instead of ITransaction

      _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                       .Returns(Task.CompletedTask);

      // Act
      var result = await _employeeService.CreateEmployeeAsync(employeeDto);

      // Assert
      Assert.NotNull(result);
      Assert.Equal("John", result.Name);
      Assert.Equal("Smith", result.Surname);
      _emailServiceMock.Verify(e => e.SendEmailAsync(
          employeeDto.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }


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
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        Branch = Branch.Johannesburg,
        City = "Johannesburg",
        ZipCode = "2000",
        Title = Title.Ms,
        Gender = Gender.Female,
        PositionId = 1,
        MonthlySalary = 30000,
        EmploymentStatus = EmploymentStatus.Permanent,
        CareerManagerID = "Manager Name",
        ProfileImage = "profile.jpg"
      };

      _employeeRepoMock.Setup(r => r.GetEmployeeByEmailAsync(employeeDto.Email))
                       .ReturnsAsync(new Employee { Email = employeeDto.Email });

      await Assert.ThrowsAsync<BusinessRuleException>(() =>
          _employeeService.CreateEmployeeAsync(employeeDto));
    }


    [Fact]
    public async Task CreateEmployeeAsyncInvalidTitleGenderThrowsValidationException()
    {
      var employeeDto = new CreateEmployeeRequestDto
      {
        Name = "Alex",
        Surname = "King",
        Title = Title.Mr,
        Gender = Gender.Female, // invalid
        Email = "alex.king@singular.co.za",
        ContactNumber = "0123456789",
        PhysicalAddress = "123 Main St",
        IdNumber = "0305054589589",
        TaxNumber = "1234567890",
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        Branch = Branch.Johannesburg,
        City = "Johannesburg",
        ZipCode = "2000",
        PositionId = 1,
        MonthlySalary = 20000,
        EmploymentStatus = EmploymentStatus.Permanent,
        CareerManagerID = "",
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
        PassportNumber = "",
        ContactNumber = "0987654321",
        Email = "updated@singular.co.za",
        PhysicalAddress = "456 New Street",
        City = "Cape Town",
        ZipCode = "8000",
        HasDisability = false,
        DisabilityDescription = null,
        Branch = Branch.Johannesburg,
        MonthlySalary = 35000,
        PositionId = 2,
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
      .ReturnsAsync(newManager);

      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employeeId))
                       .ReturnsAsync(existingEmployee);

      _employeeRepoMock.Setup(r => r.UpdateEmployeeAsync(It.IsAny<Employee>()))
                       .ReturnsAsync((Employee e) => e);


      // Act
      var result = await _employeeService.UpdateEmployeeAsync(employeeId, updateDto);

      // Assert
      Assert.NotNull(result);
      Assert.Equal("UpdatedName", result.Name);
      Assert.Equal("UpdatedSurname", result.Surname);
    }


    [Fact]
    public async Task UpdateEmployeeAsyncEmployeeNotFoundThrowsNotFoundException()
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
        PhysicalAddress = "123 Street",
        City = "Johannesburg",
        ZipCode = "2000",
        Branch = Branch.Johannesburg,
        MonthlySalary = 20000,
        PositionId = 1,
        EmploymentStatus = EmploymentStatus.Permanent,
        CareerManagerID = "Manager",
        ProfileImage = "profile.jpg"
      };

      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employeeId))
                       .ReturnsAsync((Employee?)null);

      await Assert.ThrowsAsync<NotFoundException>(() =>
          _employeeService.UpdateEmployeeAsync(employeeId, updateDto));
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

  }
}
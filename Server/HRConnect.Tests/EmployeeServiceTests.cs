namespace HRConnect.Tests
{
  using System;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.Employee;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Services;
  using HRConnect.Api.Utils;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Moq;

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
      };
    }
  }
}
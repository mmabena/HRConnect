namespace HRConnect.Tests.Services
{
  using System;
  using System.Collections.Generic;
  using Microsoft.AspNetCore.DataProtection;
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

  public class EmployeeUserProvisioningTests : IDisposable
  {
    private readonly ApplicationDBContext _context;
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IPositionRepository> _positionRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILeaveBalanceService> _leaveBalanceServiceMock;
    private readonly Mock<ILeaveProcessingService> _leaveProcessingServiceMock;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly Mock<IDbContextTransaction> _transactionMock;
    private readonly EmployeeService _employeeService;

    public EmployeeUserProvisioningTests()
    {
      var options = new DbContextOptionsBuilder<ApplicationDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
      var mockProvider = new Mock<IDataProtectionProvider>();
      // Setup CreateProtector to return a dummy protector
      var mockProtector = new Mock<IDataProtector>();
      mockProtector.Setup(p => p.Protect(It.IsAny<byte[]>())).Returns<byte[]>(b => b);
      mockProtector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Returns<byte[]>(b => b);
      mockProvider.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(mockProtector.Object);

      _context = new ApplicationDBContext(options, mockProtector.Object);
      var activeCompanyServiceMock = new Mock<IActiveCompanyService>();
      var userCompanyServiceMock = new Mock<IUserCompanyService>();
      var companyRepoMock = new Mock<ICompanyRepository>();
      _employeeRepositoryMock = new Mock<IEmployeeRepository>(MockBehavior.Strict);
      _positionRepositoryMock = new Mock<IPositionRepository>(MockBehavior.Strict);
      _emailServiceMock = new Mock<IEmailService>(MockBehavior.Strict);
      _leaveBalanceServiceMock = new Mock<ILeaveBalanceService>(MockBehavior.Strict);
      _leaveProcessingServiceMock = new Mock<ILeaveProcessingService>(MockBehavior.Strict);
      _passwordHasherMock = new Mock<IPasswordHasher<User>>(MockBehavior.Strict);
      _transactionMock = new Mock<IDbContextTransaction>(MockBehavior.Strict);

      _transactionMock
        .Setup(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
      _transactionMock
        .Setup(transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
      _transactionMock
        .Setup(transaction => transaction.Dispose());

      activeCompanyServiceMock
        .Setup(x => x.GetActiveCompanyIdAsync(It.IsAny<int>()))
        .ReturnsAsync("COMP001");

      companyRepoMock
          .Setup(x => x.GetCompanyByIdAsync("COMP001"))
          .ReturnsAsync(new Company { CompanyId = "COMP001" });

      _employeeRepositoryMock
        .Setup(repository => repository.BeginTransactionAsync())
        .ReturnsAsync(_transactionMock.Object);
      _employeeRepositoryMock
        .Setup(repository => repository.GetEmployeeByEmailAsync(It.IsAny<string>()))
        .ReturnsAsync((Employee?)null);
      _employeeRepositoryMock
        .Setup(repository => repository.GetEmployeeByTaxNumberAsync(It.IsAny<string>()))
        .ReturnsAsync((Employee?)null);
      _employeeRepositoryMock
        .Setup(repository => repository.GetEmployeeByIdNumberAsync(It.IsAny<string>()))
        .ReturnsAsync((Employee?)null);
      _employeeRepositoryMock
        .Setup(repository => repository.GetEmployeeByContactNumberAsync(It.IsAny<string>()))
        .ReturnsAsync((Employee?)null);
      _employeeRepositoryMock
        .Setup(repository => repository.GetAllEmployeeIdsWithPrefix(It.IsAny<string>()))
        .ReturnsAsync(new List<string>());
      _employeeRepositoryMock
        .Setup(repository => repository.CreateEmployeeAsync(It.IsAny<Employee>()))
        .ReturnsAsync((Employee employee) => employee);

      _positionRepositoryMock
        .Setup(repository => repository.GetPositionByIdAsync(1))
        .ReturnsAsync(new Position
        {
          PositionId = 1,
          PositionTitle = "Software Engineer",
          JobGradeId = 1,
          OccupationalLevelId = 1,
        });

      _leaveBalanceServiceMock
        .Setup(service => service.InitializeEmployeeLeaveBalancesAsync(It.IsAny<string>()))
        .Returns(Task.CompletedTask);
      _leaveBalanceServiceMock
        .Setup(service => service.RecalculateAnnualLeaveAsync(It.IsAny<string>()))
        .Returns(Task.CompletedTask);

      _emailServiceMock
        .Setup(service => service.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .Returns(Task.CompletedTask);

      _passwordHasherMock
        .Setup(hasher => hasher.HashPassword(It.IsAny<User>(), It.IsAny<string>()))
        .Returns((User user, string _) => $"hashed::{user.Email}");

      _employeeService = new EmployeeService(
        _context,
        activeCompanyServiceMock.Object,
        userCompanyServiceMock.Object,
        _employeeRepositoryMock.Object,
        _emailServiceMock.Object,
        companyRepoMock.Object,
        _positionRepositoryMock.Object,
        _leaveBalanceServiceMock.Object,
        _leaveProcessingServiceMock.Object,
        _passwordHasherMock.Object);
    }

    [Fact]
    public async Task CreateEmployeeAsyncCreatesMatchingNormalUserRecord()
    {
      var request = new CreateEmployeeRequestDto
      {
        Title = Title.Ms,
        Name = "Nomsa",
        Surname = "Dube",
        PassportNumber = "A12345678",
        Nationality = "Zimbabwean",
        Gender = Gender.Female,
        ContactNumber = "0123456789",
        TaxNumber = "1234567890",
        Email = "nomsa.dube@singular.co.za",
        PhysicalAddress = "1 Main Road",
        City = "Johannesburg",
        CompanyId = "COMP001",
        ZipCode = "2000",
        DateOfBirth = new DateOnly(1990, 1, 1),
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
        Branch = Branch.Johannesburg,
        MonthlySalary = 35000m,
        PositionId = 1,
        EmploymentStatus = EmploymentStatus.Permanent,
        ProfileImage = "profile.jpg",
      };

      var result = await _employeeService.CreateEmployeeAsync(1, request);
      var createdUser = await _context.Users.SingleAsync(user => user.Email == "nomsa.dube@singular.co.za");

      Assert.NotNull(result);
      Assert.Equal(UserRole.NormalUser, createdUser.Role);
      Assert.Equal("hashed::nomsa.dube@singular.co.za", createdUser.PasswordHash);

      _transactionMock.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
      _leaveBalanceServiceMock.Verify(service => service.InitializeEmployeeLeaveBalancesAsync(result.EmployeeId), Times.Once);
      _leaveBalanceServiceMock.Verify(service => service.RecalculateAnnualLeaveAsync(result.EmployeeId), Times.Once);
      _emailServiceMock.Verify(service => service.SendEmailAsync(
        "nomsa.dube@singular.co.za",
        "Welcome to HRConnect",
        It.Is<string>(body => body.Contains("default NormalUser role"))), Times.Once);
    }

    public void Dispose()
    {
      _context.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}
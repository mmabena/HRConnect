namespace HRConnect.Tests
{
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.Payroll.Pension;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Services;
  using Microsoft.EntityFrameworkCore;
  using Moq;
  using Xunit;

  public class PensionDeductionServiceTests
  {
    private readonly PensionDeductionService _pensionDeductionServiceMock;
    private readonly Mock<IPensionDeductionRepository> _pensionDeductionRepositoryMock;
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IEmployeePensionEnrollmentRepository> _employeePensionEnrollmentRepositoryMock;
    private readonly Mock<IPayrollRunRepository> _payrollRunRepositoryMock;
    private readonly Mock<IPayrollRunService> _payrollRunServiceMock;
    private readonly ApplicationDBContext _context;

    public PensionDeductionServiceTests()
    {
      _pensionDeductionRepositoryMock = new Mock<IPensionDeductionRepository>();
      _employeeRepositoryMock = new Mock<IEmployeeRepository>();
      _employeePensionEnrollmentRepositoryMock = new Mock<IEmployeePensionEnrollmentRepository>();
      _payrollRunRepositoryMock = new Mock<IPayrollRunRepository>();
      _payrollRunServiceMock = new Mock<IPayrollRunService>();
      //_context = new Mock<ApplicationDBContext>();
      DbContextOptions<ApplicationDBContext> options = new DbContextOptionsBuilder<ApplicationDBContext>()
        .UseInMemoryDatabase("TestDb")
        .Options;
      _context = new ApplicationDBContext(options);

      _ = _context.PensionOptions.Add(new PensionOption
      {
        ContributionPercentage = 2.50M
      });

      _pensionDeductionServiceMock = new PensionDeductionService(
        _pensionDeductionRepositoryMock.Object,
        _employeeRepositoryMock.Object,
        _employeePensionEnrollmentRepositoryMock.Object,
        _payrollRunRepositoryMock.Object,
        _payrollRunServiceMock.Object,
        _context
      );
    }

    [Fact]
    public async Task AddPensionDeductionAsyncReturnsCreatedPensionDeduction()
    {
      // Arrange
      PensionDeductionAddDto pensionDeductionAddDto = new()
      {
        EmployeeId = "EMP001",
      };

      // Fake employee so the service doesn't throw EmployeeNotFoundException
      Employee fakeEmployee = new()
      {
        EmployeeId = "EMP001",
        Name = "Test User",
        Surname = "Smith",
        PensionOptionId = 1,
        MonthlySalary = 5100.00M,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        IdNumber = "0305055487589",
        TaxNumber = "1234567890",
        PhysicalAddress = "123 Main St",
        Email = "john.smith@singular.co.za",
      };

      EmployeePensionEnrollment employeePensionEnrollment = new()
      {
        EmployeeId = "EMP001",
        PensionOptionId = 1,
        EffectiveDate = DateOnly.FromDateTime(DateTime.Now),
      };

      PensionDeduction pensionDeduction = new()
      {
        EmployeeId = "EMP001",
        FirstName = "Test User",
        LastName = "Smith",
        IdNumber = "0305055487589",
        TaxNumber = "1234567890",
        DateJoinedCompany = DateOnly.FromDateTime(DateTime.UtcNow),
        PensionableSalary = 5100.00M,
        PensionOptionId = 1,
        PendsionCategoryPercentage = 2.50M,
        PensionContribution = 127.50M,
        VoluntaryContribution = 10.00M,
        TotalPensionContribution = 137.50M,
        PhysicalAddress = "123 Main St",
        EmailAddress = "john.smith@singular.co.za",
        PayrollRunId = 1,
        CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
        IsActive = true,
      };

      _ = _employeeRepositoryMock
          .Setup(r => r.GetEmployeeByIdAsync("EMP001"))
          .ReturnsAsync(fakeEmployee);

      _ = _employeePensionEnrollmentRepositoryMock
          .Setup(r => r.GetByEmployeeIdAndLastRunIdAsync("EMP001"))
          .ReturnsAsync(employeePensionEnrollment);

      _ = _pensionDeductionRepositoryMock
          .Setup(r => r.AddAsync(It.IsAny<PensionDeduction>()))
          .ReturnsAsync(pensionDeduction);

      _ = _payrollRunRepositoryMock
          .Setup(r => r.GetCurrentRunAsync())
          .ReturnsAsync(new PayrollRun { PayrollRunId = 1 });

      // Act
      PensionDeductionDto? result = await _pensionDeductionServiceMock.AddPensionDeductionAsync(pensionDeductionAddDto);

      // Assert
      _ = Assert.IsType<PensionDeductionDto>(result);
      Assert.Equal(pensionDeduction.EmployeeId, result.EmployeeId);
      Assert.Equal(pensionDeduction.VoluntaryContribution, result.VoluntaryContribution);
      Assert.Equal(pensionDeduction.CreatedDate, result.CreatedDate);
    }


    [Fact]
    public async Task GetAllPensionDeductionsAsyncReturnsListOfPensionDeductions()
    {
      //Arrange
      List<PensionDeduction> pensionDeductionsList =
      [
        new() {
            EmployeeId = "EMP001",
            FirstName = "Test",
            LastName = "User",
            VoluntaryContribution = 5000M,
            PayrollRunId = 1,
            CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true
        },
        new() {
            EmployeeId = "EMP002",
            FirstName = "Jane",
            LastName = "Doe",
            VoluntaryContribution = 3000M,
            PayrollRunId = 2,
            CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true
        }
      ];

      _ = _pensionDeductionRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(pensionDeductionsList);

      //Act
      List<PensionDeductionDto> result = await _pensionDeductionServiceMock.GetAllPensionDeductionsAsync();
      //Assert
      Assert.NotNull(result);
      _ = Assert.IsType<List<PensionDeductionDto>>(result);
      Assert.Equal(2, result.Count);
      Assert.Equal("EMP001", result[0].EmployeeId);
      Assert.Equal("EMP002", result[1].EmployeeId);
    }

    [Fact]
    public async Task GetEmployeePensionDeductionByIdAsyncReturnsEmployeePensionDeduction()
    {
      //Arrange
      string employeeId = "EMP001";

      PensionDeduction pensionDeduction = new()
      {
        EmployeeId = employeeId,
        FirstName = "Test",
        LastName = "User",
        VoluntaryContribution = 200M,
        PayrollRunId = 1,
        CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
        IsActive = true
      };

      _ = _pensionDeductionRepositoryMock
            .Setup(r => r.GetByEmployeeIdAsync(employeeId))
            .ReturnsAsync(pensionDeduction);

      //Act
      PensionDeductionDto? result = await _pensionDeductionServiceMock.GetEmployeePensionDeductionByIdAsync(employeeId);
      //Assert
      _ = Assert.IsType<PensionDeductionDto>(result);
      Assert.Equal(employeeId, result.EmployeeId);
      Assert.Equal(200M, result.VoluntaryContribution);
    }

    [Fact]
    public async Task GetPensionDeductionsByPayRollRunIdAsyncReturnsListOfPensionDeductionsWithMatchingPayrollRunId()
    {
      //Arrange
      int payrollRunId = 1;

      List<PensionDeduction> pensionDeductionsList = [
        new() {
            EmployeeId = "EMP001",
            FirstName = "Test",
            LastName = "User",
            VoluntaryContribution = 200M,
            PayrollRunId = payrollRunId,
            CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true
        },
        new() {
            EmployeeId = "EMP002",
            FirstName = "Jane",
            LastName = "Doe",
            VoluntaryContribution = 300M,
            PayrollRunId = payrollRunId,
            CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true
        },
        new() {
            EmployeeId = "EMP003",
            FirstName = "John",
            LastName = "Smith",
            VoluntaryContribution = 400M,
            PayrollRunId = 2, // Different PayrollRunId
            CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true
        }
      ];

      _ = _pensionDeductionRepositoryMock
            .Setup(r => r.GetByPayRollRunIdAsync(payrollRunId))
            .ReturnsAsync(pensionDeductionsList.Where(pd => pd.PayrollRunId == payrollRunId).ToList());

      //Act
      List<PensionDeductionDto> result = await _pensionDeductionServiceMock.GetPensionDeductionsByPayRollRunIdAsync(payrollRunId);
      //Assert
      _ = Assert.IsType<List<PensionDeductionDto>>(result);
      Assert.Equal(2, result.Count);
      foreach (PensionDeductionDto pensionDeduction in result)
      {
        Assert.Equal(payrollRunId, pensionDeduction.PayrollRunId);
      }
    }

    [Fact]
    public async Task UpdateEmployeePensionDeductionAsyncReturnsUpdatedPensionDeductionRecord()
    {
      //Arrange
      PensionDeductionUpdateDto pensionDeductionUpdateDto = new()
      {
        EmployeeId = "EMP001",
        VoluntaryContribution = 200.00M,
        IsActive = true,
      };

      Employee fakeEmployee = new()
      {
        EmployeeId = "EMP001",
        Name = "Test User",
        Surname = "Smith",
        PensionOptionId = 1,
        MonthlySalary = 5100.00M,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        IdNumber = "0305055487589",
        TaxNumber = "1234567890",
        PhysicalAddress = "123 Main St",
        Email = "john.smith@singular.co.za",
      };

      EmployeePensionEnrollment employeePensionEnrollment = new()
      {
        EmployeeId = "EMP001",
        PensionOptionId = 1,
        EffectiveDate = DateOnly.FromDateTime(DateTime.Now),
        VoluntaryContribution = 100.00M,
        IsVoluntaryContributionPermament = false,
      };

      PensionDeduction updatedPensionDeduction = new()
      {
        EmployeeId = pensionDeductionUpdateDto.EmployeeId,
        FirstName = "Test",
        LastName = "User",
        VoluntaryContribution = (decimal)pensionDeductionUpdateDto.VoluntaryContribution,
        CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
        IsActive = (bool)pensionDeductionUpdateDto.IsActive,
        PayrollRunId = 1,
        PensionOptionId = 1,
        PensionContribution = 4000M,
        TotalPensionContribution = 4200M
      };

      _ = _employeeRepositoryMock
          .Setup(r => r.GetEmployeeByIdAsync(pensionDeductionUpdateDto.EmployeeId))
          .ReturnsAsync(fakeEmployee);

      _ = _pensionDeductionRepositoryMock
        .Setup(r => r.GetByEmployeeIdAndIsNotLockedAsync(pensionDeductionUpdateDto.EmployeeId))
        .ReturnsAsync(updatedPensionDeduction);

      _ = _employeePensionEnrollmentRepositoryMock
          .Setup(r => r.GetByEmployeeIdAndIsNotLockedAsync(pensionDeductionUpdateDto.EmployeeId))
          .ReturnsAsync(employeePensionEnrollment);

      _ = _pensionDeductionRepositoryMock
        .Setup(r => r.UpdateAsync(It.IsAny<PensionDeduction>()))
        .ReturnsAsync(updatedPensionDeduction);

      //Act
      PensionDeductionDto? result = await _pensionDeductionServiceMock.UpdateEmployeePensionDeductionAsync(pensionDeductionUpdateDto);
      //Assert
      _ = Assert.IsType<PensionDeductionDto>(result);
      Assert.Equal(pensionDeductionUpdateDto.EmployeeId, result.EmployeeId);
      Assert.Equal(pensionDeductionUpdateDto.VoluntaryContribution, result.VoluntaryContribution);
    }
  }
}

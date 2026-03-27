namespace HRConnect.Tests
{
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Services;
  using Microsoft.EntityFrameworkCore;
  using Moq;
  using Quartz;

  public class EmployeePensionEnrollmentServiceTests
  {
    private readonly EmployeePensionEnrollmentService _employeePensionEnrollmentServiceMock;
    private readonly Mock<IEmployeePensionEnrollmentRepository> _employeePensionEnrollmentRepositoryMock;
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IPayrollRunRepository> _payrollRunRepositoryMock;
    private readonly Mock<IPensionDeductionRepository> _pensionDeductionRepositoryMock;
    private readonly Mock<IPayrollRunService> _payrollRunServiceMock;
    private readonly Mock<ISchedulerFactory> _scheduler;
    private readonly ApplicationDBContext _context;

    public EmployeePensionEnrollmentServiceTests()
    {
      _employeePensionEnrollmentRepositoryMock = new Mock<IEmployeePensionEnrollmentRepository>();
      _employeeRepositoryMock = new Mock<IEmployeeRepository>();
      _payrollRunRepositoryMock = new Mock<IPayrollRunRepository>();
      _pensionDeductionRepositoryMock = new Mock<IPensionDeductionRepository>();
      _payrollRunServiceMock = new Mock<IPayrollRunService>();
      _scheduler = new Mock<ISchedulerFactory>();
      DbContextOptions<ApplicationDBContext> options = new DbContextOptionsBuilder<ApplicationDBContext>()
        .UseInMemoryDatabase("TestDb")
        .Options;
      _context = new ApplicationDBContext(options);

      _ = _context.PensionOptions.Add(new PensionOption
      {
        ContributionPercentage = 2.50M
      });

      _employeePensionEnrollmentServiceMock = new EmployeePensionEnrollmentService(
        _employeePensionEnrollmentRepositoryMock.Object,
        _employeeRepositoryMock.Object,
        _payrollRunRepositoryMock.Object,
        _pensionDeductionRepositoryMock.Object,
        _scheduler.Object,
        _context
        );
    }

    [Fact]
    public async Task AddEmployeePensionEnrollmentAsyncReturnsCreatedEmployeePensionEnrollmentDto()
    {
      //Arrange
      DateOnly effectiveDate = DateOnly.FromDateTime(DateTime.UtcNow);
      if (effectiveDate.Day > 15)
      {
        effectiveDate = new DateOnly(effectiveDate.Year, effectiveDate.Month, 1).AddMonths(1);
      }

      EmployeePensionEnrollmentAddDto employeePensionEnrollmentAddDto = new()
      {
        EmployeeId = "EMP001",
        EffectiveDate = effectiveDate,
        VoluntaryContribution = 200M,
        IsVoluntaryContributionPermament = false
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
        EffectiveDate = employeePensionEnrollmentAddDto.EffectiveDate,
        VoluntaryContribution = employeePensionEnrollmentAddDto.VoluntaryContribution ?? 0M,
        IsVoluntaryContributionPermament = employeePensionEnrollmentAddDto.IsVoluntaryContributionPermament,
        IsLocked = true
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
          .Setup(r => r.GetEmployeeByIdAsync(employeePensionEnrollmentAddDto.EmployeeId))
          .ReturnsAsync(fakeEmployee);

      _ = _employeePensionEnrollmentRepositoryMock
          .Setup(r => r.GetByEmployeeIdAndLastRunIdAsync(employeePensionEnrollmentAddDto.EmployeeId))
          .ReturnsAsync(employeePensionEnrollment);

      _ = _payrollRunRepositoryMock
          .Setup(r => r.GetCurrentRunAsync())
          .ReturnsAsync(new PayrollRun { PayrollRunId = 1 });

      _ = _employeePensionEnrollmentRepositoryMock
          .Setup(r => r.AddAsync(It.IsAny<EmployeePensionEnrollment>()))
          .ReturnsAsync(employeePensionEnrollment);

      Mock<IScheduler> schedulerMock = new();
      _ = schedulerMock
          .Setup(s => s.ScheduleJob(
            It.IsAny<IJobDetail>(),
            It.IsAny<ITrigger>(),
            It.IsAny<CancellationToken>()))
          .ReturnsAsync(DateTimeOffset.UtcNow);

      _ = _scheduler
          .Setup(s => s.GetScheduler(It.IsAny<CancellationToken>()))
          .ReturnsAsync(schedulerMock.Object);


      //Act
      EmployeePensionEnrollmentDto result = await _employeePensionEnrollmentServiceMock.AddEmployeePensionEnrollmentAsync(employeePensionEnrollmentAddDto);
      //Assert
      _ = Assert.IsType<EmployeePensionEnrollmentDto>(result);
      Assert.Equal(employeePensionEnrollmentAddDto.EmployeeId, result.EmployeeId);
      Assert.Equal(employeePensionEnrollmentAddDto.EffectiveDate, result.EffectiveDate);
      Assert.Equal(employeePensionEnrollmentAddDto.VoluntaryContribution, result.VoltunaryContribution);
    }

    [Fact]
    public async Task GetAllEmployeePensionEnrollementsAsyncReturnsAllEmployeePensionEnrollmentDto()
    {
      //Arrange
      List<EmployeePensionEnrollment> employeePensionEnrollments =
      [
        new EmployeePensionEnrollment
        {
          EmployeeId = "EMP001",
          PensionOptionId = 1,
          EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
          VoluntaryContribution = 200M,
          IsVoluntaryContributionPermament = false,
          IsLocked = false
        },
        new EmployeePensionEnrollment
        {
          EmployeeId = "EMP002",
          PensionOptionId = 1,
          EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
          VoluntaryContribution = 300M,
          IsVoluntaryContributionPermament = true,
          IsLocked = false
        }
      ];

      _ = _employeePensionEnrollmentRepositoryMock
          .Setup(r => r.GetAllAsync())
          .ReturnsAsync(employeePensionEnrollments);

      //Act
      List<EmployeePensionEnrollmentDto> result = await _employeePensionEnrollmentServiceMock.GetAllEmployeePensionEnrollementsAsync();
      //Assert
      _ = Assert.IsType<List<EmployeePensionEnrollmentDto>>(result);
      Assert.Equal(2, result.Count);
      Assert.Equal("EMP001", result[0].EmployeeId);
      Assert.Equal("EMP002", result[1].EmployeeId);
    }

    [Fact]
    public async Task GetEmployeePensionEnrollementByIdAsyncEmployeesLatestPensionEnrollment()
    {
      //Arrange
      string employeeId = "EMP001";
      EmployeePensionEnrollment employeePensionEnrollment = new()
      {
        EmployeeId = employeeId,
        PensionOptionId = 1,
        EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
        VoluntaryContribution = 200M,
        IsVoluntaryContributionPermament = false,
        IsLocked = false
      };

      _ = _employeePensionEnrollmentRepositoryMock
          .Setup(r => r.GetByEmployeeIdAndLastRunIdAsync(employeeId))
          .ReturnsAsync(employeePensionEnrollment);

      //Act
      EmployeePensionEnrollmentDto? result = await _employeePensionEnrollmentServiceMock.
        GetEmployeePensionEnrollementByIdAsync(employeeId);
      //Assert
      _ = Assert.IsType<EmployeePensionEnrollmentDto>(result);
      Assert.Equal(employeeId, result.EmployeeId);
      Assert.Equal(employeePensionEnrollment.PensionOptionId, result.PensionOptionId);
      Assert.Equal(employeePensionEnrollment.VoluntaryContribution, result.VoltunaryContribution);
    }

    [Fact]
    public async Task GetPensionEnrollementsByPayRollRunIdAsyncReturnsListOfPensionEnrollmentsWithMatchingPayrollRunId()
    {
      //Arrange
      int payrollRunId = 1;
      List<EmployeePensionEnrollment> employeePensionEnrollments =
      [
        new EmployeePensionEnrollment
        {
          EmployeeId = "EMP001",
          PensionOptionId = 1,
          EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
          VoluntaryContribution = 200M,
          IsVoluntaryContributionPermament = false,
          IsLocked = true,
          PayrollRunId = 1
        },
        new EmployeePensionEnrollment
        {
          EmployeeId = "EMP002",
          PensionOptionId = 1,
          EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
          VoluntaryContribution = 300M,
          IsVoluntaryContributionPermament = true,
          IsLocked = true,
          PayrollRunId = 1
        },
        new EmployeePensionEnrollment
        {
          EmployeeId = "EMP001",
          PensionOptionId = 1,
          EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
          VoluntaryContribution = 300M,
          IsVoluntaryContributionPermament = true,
          IsLocked = false,
          PayrollRunId = 2 //Different PayrollRunId 
        }
      ];

      _ = _employeePensionEnrollmentRepositoryMock
          .Setup(r => r.GetByPayRollRunIdAsync(payrollRunId))
          .ReturnsAsync(employeePensionEnrollments.Where(e => e.PayrollRunId == payrollRunId).ToList());

      //Act
      List<EmployeePensionEnrollmentDto> result = await _employeePensionEnrollmentServiceMock.GetPensionEnrollementsByPayRollRunIdAsync(payrollRunId);

      //Assert
      _ = Assert.IsType<List<EmployeePensionEnrollmentDto>>(result);
      Assert.Equal(2, result.Count);
      foreach (EmployeePensionEnrollmentDto enrollment in result)
      {
        Assert.Equal(payrollRunId, enrollment.PayrollRunId);
      }
    }

    [Fact]
    public async Task GetPensionEnrollementsNotLocked()
    {
      //Arrange
      List<EmployeePensionEnrollment> employeePensionEnrollments =
      [
        new EmployeePensionEnrollment
        {
          EmployeeId = "EMP001",
          PensionOptionId = 1,
          EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
          VoluntaryContribution = 200M,
          IsVoluntaryContributionPermament = false,
          IsLocked = true,
          PayrollRunId = 1
        },
        new EmployeePensionEnrollment
        {
          EmployeeId = "EMP002",
          PensionOptionId = 1,
          EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
          VoluntaryContribution = 300M,
          IsVoluntaryContributionPermament = true,
          IsLocked = true,
          PayrollRunId = 1
        },
        new EmployeePensionEnrollment
        {
          EmployeeId = "EMP001",
          PensionOptionId = 1,
          EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
          VoluntaryContribution = 300M,
          IsVoluntaryContributionPermament = true,
          IsLocked = false,
          PayrollRunId = 2 //Different PayrollRunId 
        },
        new EmployeePensionEnrollment
        {
          EmployeeId = "EMP002",
          PensionOptionId = 1,
          EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
          VoluntaryContribution = 300M,
          IsVoluntaryContributionPermament = true,
          IsLocked = false,
          PayrollRunId = 2 //Different PayrollRunId
        }
      ];

      _ = _employeePensionEnrollmentRepositoryMock
          .Setup(r => r.GetEmployeePensionEnrollmentsNotLocked())
          .ReturnsAsync(employeePensionEnrollments.Where(e => !e.IsLocked).ToList());

      //Act
      List<EmployeePensionEnrollmentDto> result = await _employeePensionEnrollmentServiceMock.GetPensionEnrollementsNotLocked();
      //Assert
      _ = Assert.IsType<List<EmployeePensionEnrollmentDto>>(result);
      Assert.Equal(2, result.Count);
      foreach (EmployeePensionEnrollmentDto enrollment in result)
      {
        Assert.NotNull(enrollment.EmployeeId);
        Assert.NotEqual(0, enrollment.PensionOptionId);
        Assert.NotEqual(0, enrollment.PayrollRunId);
      }
    }

    [Fact]
    public async Task UpdateEmployeePensionEnrollementAsync()
    {
      //Arrange
      EmployeePensionEnrollmentUpdateDto employeePensionEnrollmentUpdateDto = new()
      {
        EmployeeId = "EMP001",
        PensionOptionId = 1,
        VoluntaryContribution = 300M,
        IsVoluntaryContributionPermament = true,
      };

      EmployeePensionEnrollment employeePensionEnrollment = new()
      {
        EmployeeId = "EMP001",
        PensionOptionId = 1,
        EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
        VoluntaryContribution = employeePensionEnrollmentUpdateDto.VoluntaryContribution ?? 0M,
        IsVoluntaryContributionPermament = employeePensionEnrollmentUpdateDto.IsVoluntaryContributionPermament,
        IsLocked = false
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

      _ = _employeePensionEnrollmentRepositoryMock
          .Setup(r => r.GetByEmployeeIdAndLastRunIdAsync(employeePensionEnrollmentUpdateDto.EmployeeId))
          .ReturnsAsync(employeePensionEnrollment);

      _ = _employeeRepositoryMock
          .Setup(r => r.GetEmployeeByIdAsync(employeePensionEnrollmentUpdateDto.EmployeeId))
          .ReturnsAsync(fakeEmployee);

      Mock<IScheduler> schedulerMock = new();
      _ = schedulerMock
          .Setup(s => s.ScheduleJob(
            It.IsAny<IJobDetail>(),
            It.IsAny<ITrigger>(),
            It.IsAny<CancellationToken>()))
          .ReturnsAsync(DateTimeOffset.UtcNow);

      _ = _scheduler
          .Setup(s => s.GetScheduler(It.IsAny<CancellationToken>()))
          .ReturnsAsync(schedulerMock.Object);

      _ = _employeePensionEnrollmentRepositoryMock
          .Setup(r => r.UpdateAsync(It.IsAny<EmployeePensionEnrollment>()))
          .ReturnsAsync(employeePensionEnrollment);

      //Act
      EmployeePensionEnrollmentDto result = await _employeePensionEnrollmentServiceMock.UpdateEmployeePensionEnrollementAsync(employeePensionEnrollmentUpdateDto);

      //Assert
      _ = Assert.IsType<EmployeePensionEnrollmentDto>(result);
      Assert.Equal(employeePensionEnrollmentUpdateDto.EmployeeId, result.EmployeeId);
      Assert.Equal(employeePensionEnrollmentUpdateDto.PensionOptionId, result.PensionOptionId);
      Assert.Equal(employeePensionEnrollmentUpdateDto.VoluntaryContribution, result.VoltunaryContribution);
      Assert.Equal(employeePensionEnrollmentUpdateDto.IsVoluntaryContributionPermament, result.IsVoluntaryContributionPermament);
    }
  }
}

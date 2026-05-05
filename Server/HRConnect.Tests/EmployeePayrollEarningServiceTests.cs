namespace HRConnect.Tests
{
  using HRConnect.Api.DTOs.Payroll.Earning;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.Payroll.Earning;
  using HRConnect.Api.Services;
  using Moq;

  public class EmployeePayrollEarningServiceTests
  {
    private readonly EmployeePayrollEarningService _employeePayrollEarningServiceMock;
    private readonly Mock<IEmployeePayrollEarningRepository> _employeePayrollEarningRepositoryMock;
    private readonly Mock<IPayrollRunRepository> _payrollRunRepositoryMock;
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IPayrollEarningRepository> _payrollEarningRepositoryMock;
    private readonly Mock<ITaxDeductionService> _taxDeductionServiceMock;

    public EmployeePayrollEarningServiceTests()
    {
      _employeePayrollEarningRepositoryMock = new Mock<IEmployeePayrollEarningRepository>();
      _payrollRunRepositoryMock = new Mock<IPayrollRunRepository>();
      _employeeRepositoryMock = new Mock<IEmployeeRepository>();
      _payrollEarningRepositoryMock = new Mock<IPayrollEarningRepository>();
      _payrollEarningRepositoryMock = new Mock<IPayrollEarningRepository>();
      _taxDeductionServiceMock = new Mock<ITaxDeductionService>();

      _employeePayrollEarningServiceMock = new EmployeePayrollEarningService(
        _employeePayrollEarningRepositoryMock.Object,
        _payrollRunRepositoryMock.Object,
        _employeeRepositoryMock.Object,
        _payrollEarningRepositoryMock.Object,
        _taxDeductionServiceMock.Object
        );
    }

    [Fact]
    public async Task AddAsyncReturnsAddedEmployeePayrollEarning()
    {
      //Arrange
      PayrollRun currentPayrollRun = new()
      {
        PayrollRunId = 1
      };

      EmployeePayrollEarningAddDto employeePayrollEarningAddDto = new()
      {
        EmployeeId = "EMP001",
        PayrollEarningId = "PRE001",
        Amount = 1000
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

      PayrollEarning payrollEarning = new()
      {
        PayrollEarningId = "PRE001",
        ShortDescription = "Basic Salary",
        LongDescription = "Monthly base salary for employee",
        Taxable = true,
        TaxCode = 3601,
        TaxPercentage = 100m,
        OvertimeHourMultiplier = null,
        CanProRata = true,
        IsOnGoing = true,
        IsActive = true
      };

      EmployeePayrollEarning employeePayrollEarning = new()
      {
        EmployeeId = fakeEmployee.EmployeeId,
        PayrollEarningId = payrollEarning.PayrollEarningId,
        TaxCode = payrollEarning.TaxCode,
        Amount = (decimal)employeePayrollEarningAddDto.Amount,
        CalculatedAmountAfterTax = (decimal)employeePayrollEarningAddDto.Amount,
        PayrollRunId = currentPayrollRun.PayrollRunId,
        IsLocked = false
      };

      _ = _payrollRunRepositoryMock
        .Setup(r => r.GetCurrentRunAsync())
        .ReturnsAsync(currentPayrollRun);

      _ = _employeeRepositoryMock
        .Setup(r => r.GetEmployeeByIdAsync(fakeEmployee.EmployeeId))
        .ReturnsAsync(fakeEmployee);

      _ = _employeePayrollEarningRepositoryMock
        .Setup(r => r.CheckIfEmployeeEarningExistsForCurrentPayrun(fakeEmployee.EmployeeId, payrollEarning.PayrollEarningId, currentPayrollRun.PayrollRunId));

      _ = _payrollEarningRepositoryMock
        .Setup(r => r.GetByPayrollEarningId(payrollEarning.PayrollEarningId))
        .ReturnsAsync(payrollEarning);

      _ = _employeePayrollEarningRepositoryMock
        .Setup(r => r.AddAsync(It.IsAny<EmployeePayrollEarning>()))
        .ReturnsAsync(employeePayrollEarning);

      //Act
      EmployeePayrollEarningDto result = await _employeePayrollEarningServiceMock.AddAsync(employeePayrollEarningAddDto);

      //Assert
      _ = Assert.IsType<EmployeePayrollEarningDto>(result);
      Assert.Equal(employeePayrollEarning.EmployeeId, result.EmployeeId);
      Assert.Equal(employeePayrollEarning.PayrollEarningId, result.PayrollEarningId);
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllEmployeePayrollEarnings()
    {
      //Arrange
      List<EmployeePayrollEarning> employeePayrollEarnings = new()
      {
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 1,
            EmployeeId = "EMP001",
            PayrollEarningId = "PRE001",
            TaxCode = 3601,
            OverTimeHoursWorked = null,
            Amount = 25000.00m,
            CalculatedAmountAfterTax = 20000.00m,
            PayrollRunId = 100,
            IsLocked = false
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 2,
            EmployeeId = "EMP002",
            PayrollEarningId = "PRE001",
            TaxCode = 3602,
            OverTimeHoursWorked = 10,
            Amount = 5000.00m,
            CalculatedAmountAfterTax = 4000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 3,
            EmployeeId = "EMP003",
            PayrollEarningId = "PRE001",
            TaxCode = 3701,
            OverTimeHoursWorked = null,
            Amount = 3000.00m,
            CalculatedAmountAfterTax = 3000.00m, // non-taxable allowance
            PayrollRunId = 100,
            IsLocked = false
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 4,
            EmployeeId = "EMP004",
            PayrollEarningId = "PRE001",
            TaxCode = 3901,
            OverTimeHoursWorked = null,
            Amount = 15000.00m,
            CalculatedAmountAfterTax = 12000.00m,
            PayrollRunId = 100,
            IsLocked = true
        }
      };

      _ = _employeePayrollEarningRepositoryMock
        .Setup(r => r.GetAllAsync())
        .ReturnsAsync(employeePayrollEarnings);

      //Act
      List<EmployeePayrollEarningDto> result = await _employeePayrollEarningServiceMock.GetAllAsync();

      //Assert
      _ = Assert.IsType<List<EmployeePayrollEarningDto>>(result);
      Assert.Equal(employeePayrollEarnings.Count, result.Count);
      Assert.All(result, epe => Assert.Equal("PRE001", epe.PayrollEarningId));
      Assert.All(result, epe => Assert.Equal(100, epe.PayrollRunId));
    }


    [Fact]
    public async Task GetByEmployeeIdAsyncReturnsEmployeePayrollEarnings()
    {
      //Arrange
      string employeeId = "EMP001";
      List<EmployeePayrollEarning> employeePayrollEarnings = new()
      {
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 1,
            EmployeeId = "EMP001",
            PayrollEarningId = "PRE001",
            TaxCode = 3601,
            OverTimeHoursWorked = null,
            Amount = 25000.00m,
            CalculatedAmountAfterTax = 20000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 2,
            EmployeeId = "EMP002",
            PayrollEarningId = "PRE001",
            TaxCode = 3602,
            OverTimeHoursWorked = 10,
            Amount = 5000.00m,
            CalculatedAmountAfterTax = 4000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 3,
            EmployeeId = "EMP003",
            PayrollEarningId = "PRE001",
            TaxCode = 3701,
            OverTimeHoursWorked = null,
            Amount = 3000.00m,
            CalculatedAmountAfterTax = 3000.00m, // non-taxable allowance
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 4,
            EmployeeId = "EMP004",
            PayrollEarningId = "PRE001",
            TaxCode = 3901,
            OverTimeHoursWorked = null,
            Amount = 15000.00m,
            CalculatedAmountAfterTax = 12000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 1,
            EmployeeId = "EMP001",
            PayrollEarningId = "PRE001",
            TaxCode = 3601,
            OverTimeHoursWorked = null,
            Amount = 25000.00m,
            CalculatedAmountAfterTax = 20000.00m,
            PayrollRunId = 101,
            IsLocked = false
        },
      };

      _ = _employeePayrollEarningRepositoryMock
        .Setup(r => r.GetByEmployeeIdAsync(employeeId))
        .ReturnsAsync(employeePayrollEarnings.Where(epe => epe.EmployeeId == employeeId).ToList());

      //Act
      List<EmployeePayrollEarningDto> result = await _employeePayrollEarningServiceMock.GetByEmployeeIdAsync(employeeId);

      //Assert
      _ = Assert.IsType<List<EmployeePayrollEarningDto>>(result);
      Assert.Equal(2, result.Count);
      Assert.All(result, epe => Assert.Equal(employeeId, epe.EmployeeId));
    }

    [Fact]
    public async Task GetByEmployeeIdAndIsNotLockedAsyncReturnsEmployeePayrollEarningsAndNotLocked()
    {
      //Arrange
      string employeeId = "EMP001";
      List<EmployeePayrollEarning> employeePayrollEarnings = new()
      {
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 1,
            EmployeeId = "EMP001",
            PayrollEarningId = "PRE001",
            TaxCode = 3601,
            OverTimeHoursWorked = null,
            Amount = 25000.00m,
            CalculatedAmountAfterTax = 20000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 2,
            EmployeeId = "EMP002",
            PayrollEarningId = "PRE001",
            TaxCode = 3602,
            OverTimeHoursWorked = 10,
            Amount = 5000.00m,
            CalculatedAmountAfterTax = 4000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 3,
            EmployeeId = "EMP003",
            PayrollEarningId = "PRE001",
            TaxCode = 3701,
            OverTimeHoursWorked = null,
            Amount = 3000.00m,
            CalculatedAmountAfterTax = 3000.00m, // non-taxable allowance
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 4,
            EmployeeId = "EMP004",
            PayrollEarningId = "PRE001",
            TaxCode = 3901,
            OverTimeHoursWorked = null,
            Amount = 15000.00m,
            CalculatedAmountAfterTax = 12000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 1,
            EmployeeId = "EMP001",
            PayrollEarningId = "PRE001",
            TaxCode = 3601,
            OverTimeHoursWorked = null,
            Amount = 25000.00m,
            CalculatedAmountAfterTax = 20000.00m,
            PayrollRunId = 101,
            IsLocked = false
        },
      };

      _ = _employeePayrollEarningRepositoryMock
        .Setup(r => r.GetByEmployeeIdAndIsNotLockedAsync(employeeId))
        .ReturnsAsync(employeePayrollEarnings.Where(epe => epe.EmployeeId == employeeId && !epe.IsLocked).ToList());

      //Act
      List<EmployeePayrollEarningDto> result = await _employeePayrollEarningServiceMock.GetByEmployeeIdAndIsNotLockedAsync(employeeId);

      //Assert
      _ = Assert.IsType<List<EmployeePayrollEarningDto>>(result);
      _ = Assert.Single(result);
      Assert.Equal(employeeId, result[0].EmployeeId);
    }

    [Fact]
    public async Task GetByEmployeeIdAndLastRunIdAsyncReturnsEmployeesPayrollEarningsForCurrentPayrollRun()
    {
      //Arrange
      string employeeId = "EMP001";
      int latestPayrollRunId = 101;
      List<EmployeePayrollEarning> employeePayrollEarnings = new()
      {
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 1,
            EmployeeId = "EMP001",
            PayrollEarningId = "PRE001",
            TaxCode = 3601,
            OverTimeHoursWorked = null,
            Amount = 25000.00m,
            CalculatedAmountAfterTax = 20000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 2,
            EmployeeId = "EMP002",
            PayrollEarningId = "PRE001",
            TaxCode = 3602,
            OverTimeHoursWorked = 10,
            Amount = 5000.00m,
            CalculatedAmountAfterTax = 4000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 3,
            EmployeeId = "EMP003",
            PayrollEarningId = "PRE001",
            TaxCode = 3701,
            OverTimeHoursWorked = null,
            Amount = 3000.00m,
            CalculatedAmountAfterTax = 3000.00m, // non-taxable allowance
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 4,
            EmployeeId = "EMP004",
            PayrollEarningId = "PRE001",
            TaxCode = 3901,
            OverTimeHoursWorked = null,
            Amount = 15000.00m,
            CalculatedAmountAfterTax = 12000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 1,
            EmployeeId = "EMP001",
            PayrollEarningId = "PRE001",
            TaxCode = 3601,
            OverTimeHoursWorked = null,
            Amount = 25000.00m,
            CalculatedAmountAfterTax = 20000.00m,
            PayrollRunId = 101,
            IsLocked = false
        },
      };

      _ = _employeePayrollEarningRepositoryMock
        .Setup(r => r.GetByEmployeeIdAndLastRunIdAsync(employeeId))
        .ReturnsAsync(employeePayrollEarnings.Where(epe => epe.EmployeeId == employeeId && epe.PayrollRunId == latestPayrollRunId).ToList());

      //Act
      List<EmployeePayrollEarningDto> result = await _employeePayrollEarningServiceMock.GetByEmployeeIdAndLastRunIdAsync(employeeId);

      //Assert
      _ = Assert.IsType<List<EmployeePayrollEarningDto>>(result);
      _ = Assert.Single(result);
      Assert.Equal(employeeId, result[0].EmployeeId);
      Assert.Equal(latestPayrollRunId, result[0].PayrollRunId);
    }

    [Fact]
    public async Task GetByPayrollRunIdAsyncReturnsEmployeePayrollEarningsForCurrentPayrollRun()
    {
      //Arrange
      int latestPayrollRunId = 100;
      List<EmployeePayrollEarning> employeePayrollEarnings = new()
      {
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 1,
            EmployeeId = "EMP001",
            PayrollEarningId = "PRE001",
            TaxCode = 3601,
            OverTimeHoursWorked = null,
            Amount = 25000.00m,
            CalculatedAmountAfterTax = 20000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 2,
            EmployeeId = "EMP002",
            PayrollEarningId = "PRE001",
            TaxCode = 3602,
            OverTimeHoursWorked = 10,
            Amount = 5000.00m,
            CalculatedAmountAfterTax = 4000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 3,
            EmployeeId = "EMP003",
            PayrollEarningId = "PRE001",
            TaxCode = 3701,
            OverTimeHoursWorked = null,
            Amount = 3000.00m,
            CalculatedAmountAfterTax = 3000.00m, // non-taxable allowance
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 4,
            EmployeeId = "EMP004",
            PayrollEarningId = "PRE001",
            TaxCode = 3901,
            OverTimeHoursWorked = null,
            Amount = 15000.00m,
            CalculatedAmountAfterTax = 12000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 1,
            EmployeeId = "EMP001",
            PayrollEarningId = "PRE001",
            TaxCode = 3601,
            OverTimeHoursWorked = null,
            Amount = 25000.00m,
            CalculatedAmountAfterTax = 20000.00m,
            PayrollRunId = 101,
            IsLocked = false
        },
      };

      _ = _employeePayrollEarningRepositoryMock
        .Setup(r => r.GetByPayrollRunIdAsync(latestPayrollRunId))
        .ReturnsAsync(employeePayrollEarnings.Where(epe => epe.PayrollRunId == latestPayrollRunId).ToList());

      //Act
      List<EmployeePayrollEarningDto> result = await _employeePayrollEarningServiceMock.GetByPayrollRunIdAsync(latestPayrollRunId);

      //Assert
      _ = Assert.IsType<List<EmployeePayrollEarningDto>>(result);
      Assert.Equal(4, result.Count);
      Assert.All(result, epe => Assert.Equal(latestPayrollRunId, epe.PayrollRunId));
    }

    [Fact]
    public async Task GetByTaxCodeAsyncReturnsEmployeePayrollEarningsByTaxCode()
    {
      //Arrange
      int taxCode = 3601;
      List<EmployeePayrollEarning> employeePayrollEarnings = new()
      {
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 1,
            EmployeeId = "EMP001",
            PayrollEarningId = "PRE001",
            TaxCode = 3601,
            OverTimeHoursWorked = null,
            Amount = 25000.00m,
            CalculatedAmountAfterTax = 20000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 2,
            EmployeeId = "EMP002",
            PayrollEarningId = "PRE001",
            TaxCode = 3601,
            OverTimeHoursWorked = 10,
            Amount = 5000.00m,
            CalculatedAmountAfterTax = 4000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 3,
            EmployeeId = "EMP003",
            PayrollEarningId = "PRE001",
            TaxCode = 3601,
            OverTimeHoursWorked = null,
            Amount = 3000.00m,
            CalculatedAmountAfterTax = 3000.00m, // non-taxable allowance
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 4,
            EmployeeId = "EMP004",
            PayrollEarningId = "PRE001",
            TaxCode = 3601,
            OverTimeHoursWorked = null,
            Amount = 15000.00m,
            CalculatedAmountAfterTax = 12000.00m,
            PayrollRunId = 100,
            IsLocked = true
        }
      };

      _ = _employeePayrollEarningRepositoryMock
        .Setup(r => r.GetByTaxCodeAsync(taxCode))
        .ReturnsAsync(employeePayrollEarnings.Where(epe => epe.TaxCode == taxCode).ToList());

      //Act
      List<EmployeePayrollEarningDto> result = await _employeePayrollEarningServiceMock.GetByTaxCodeAsync(taxCode);

      //Assert
      _ = Assert.IsType<List<EmployeePayrollEarningDto>>(result);
      Assert.Equal(4, result.Count);
      Assert.All(result, epe => Assert.Equal(taxCode, epe.TaxCode));
    }

    [Fact]
    public async Task GetByPayrollEarningIdAsyncReturnsAllEmployeePayrollEarnings()
    {
      //Arrange
      string payrollEarningId = "PRE001";
      List<EmployeePayrollEarning> employeePayrollEarnings = new()
      {
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 1,
            EmployeeId = "EMP001",
            PayrollEarningId = "PRE001",
            TaxCode = 3601,
            OverTimeHoursWorked = null,
            Amount = 25000.00m,
            CalculatedAmountAfterTax = 20000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 2,
            EmployeeId = "EMP002",
            PayrollEarningId = "PRE001",
            TaxCode = 3602,
            OverTimeHoursWorked = 10,
            Amount = 5000.00m,
            CalculatedAmountAfterTax = 4000.00m,
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 3,
            EmployeeId = "EMP003",
            PayrollEarningId = "PRE001",
            TaxCode = 3701,
            OverTimeHoursWorked = null,
            Amount = 3000.00m,
            CalculatedAmountAfterTax = 3000.00m, // non-taxable allowance
            PayrollRunId = 100,
            IsLocked = true
        },
        new EmployeePayrollEarning
        {
            EmployeePayrollEarningId = 4,
            EmployeeId = "EMP004",
            PayrollEarningId = "PRE001",
            TaxCode = 3901,
            OverTimeHoursWorked = null,
            Amount = 15000.00m,
            CalculatedAmountAfterTax = 12000.00m,
            PayrollRunId = 100,
            IsLocked = true
        }
      };

      _ = _employeePayrollEarningRepositoryMock
        .Setup(r => r.GetByPayrollEarningIdAsync(payrollEarningId))
        .ReturnsAsync(employeePayrollEarnings.Where(epe => epe.PayrollEarningId == payrollEarningId).ToList());

      //Act
      List<EmployeePayrollEarningDto> result = await _employeePayrollEarningServiceMock.GetByPayrollEarningIdAsync(payrollEarningId);

      //Assert
      _ = Assert.IsType<List<EmployeePayrollEarningDto>>(result);
      Assert.Equal(4, result.Count);
      Assert.All(result, epe => Assert.Equal(payrollEarningId, epe.PayrollEarningId));
    }

    [Fact]
    public async Task UpdateAsyncReturnsUpdatedEmployeePayrollEarning()
    {
      //Arrange
      PayrollRun currentPayrollRun = new()
      {
        PayrollRunId = 1
      };

      EmployeePayrollEarningUpdateDto employeePayrollEarningUpdateDto = new()
      {
        EmployeeId = "EMP001",
        PayrollEarningId = "PRE002",
        Amount = 1000
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

      PayrollEarning payrollEarning = new()
      {
        PayrollEarningId = "PRE002",
        ShortDescription = "Travel Allowance",
        LongDescription = "Allowance for business travel expenses",
        Taxable = false,
        TaxCode = 3701,
        TaxPercentage = null,
        OvertimeHourMultiplier = null,
        CanProRata = false,
        IsOnGoing = true,
        IsActive = true
      };

      EmployeePayrollEarning employeePayrollEarning = new()
      {
        EmployeeId = fakeEmployee.EmployeeId,
        PayrollEarningId = payrollEarning.PayrollEarningId,
        Amount = (decimal)employeePayrollEarningUpdateDto.Amount,
        TaxCode = payrollEarning.TaxCode,
        PayrollRunId = currentPayrollRun.PayrollRunId,
        IsLocked = false,
      };

      _ = _payrollRunRepositoryMock
        .Setup(r => r.GetCurrentRunAsync())
        .ReturnsAsync(currentPayrollRun);

      _ = _employeeRepositoryMock
        .Setup(r => r.GetEmployeeByIdAsync(fakeEmployee.EmployeeId))
        .ReturnsAsync(fakeEmployee);

      _ = _employeePayrollEarningRepositoryMock
        .Setup(r => r.CheckIfEmployeeEarningExistsForCurrentPayrun(fakeEmployee.EmployeeId, payrollEarning.PayrollEarningId, currentPayrollRun.PayrollRunId))
        .ReturnsAsync(employeePayrollEarning);

      _ = _payrollEarningRepositoryMock
        .Setup(r => r.GetByPayrollEarningId(payrollEarning.PayrollEarningId))
        .ReturnsAsync(payrollEarning);

      _ = _employeePayrollEarningRepositoryMock
        .Setup(r => r.UpdateAsync(employeePayrollEarning))
        .ReturnsAsync(employeePayrollEarning);

      //Act
      EmployeePayrollEarningDto result = await _employeePayrollEarningServiceMock.UpdateAsync(employeePayrollEarningUpdateDto);

      //Assert
      _ = Assert.IsType<EmployeePayrollEarningDto>(result);
      Assert.Equal(employeePayrollEarningUpdateDto.EmployeeId, result.EmployeeId);
      Assert.Equal(employeePayrollEarningUpdateDto.PayrollEarningId, result.PayrollEarningId);
    }
  }
}

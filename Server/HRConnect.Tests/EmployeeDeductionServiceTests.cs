namespace HRConnect.Tests
{
  using HRConnect.Api.DTOs.Payroll.Deduction;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Services;
  using Moq;
  public class EmployeeDeductionServiceTests
  {
    private readonly EmployeeDeductionService _employeeDeductionServiceMock;
    private readonly Mock<IEmployeeDeductionRepository> _employeeDeductionRepositoryMock;
    private readonly Mock<IDeductionRepository> _deductionRepositoryMock;
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IPayrollRunRepository> _payrollRunRepositoryMock;

    public EmployeeDeductionServiceTests()
    {
      _employeeDeductionRepositoryMock = new Mock<IEmployeeDeductionRepository>();
      _deductionRepositoryMock = new Mock<IDeductionRepository>();
      _employeeRepositoryMock = new Mock<IEmployeeRepository>();
      _payrollRunRepositoryMock = new Mock<IPayrollRunRepository>();

      _employeeDeductionServiceMock = new EmployeeDeductionService(
        _employeeDeductionRepositoryMock.Object,
        _deductionRepositoryMock.Object,
        _employeeRepositoryMock.Object,
        _payrollRunRepositoryMock.Object
        );
    }

    [Fact]
    public async Task AddAsyncReturnsAddedEmployeeDeduction()
    {
      //Arrange
      PayrollRun currentPayrollRun = new()
      {
        PayrollRunId = 1
      };

      EmployeeDeductionAddDto employeeDeductionAddDto = new()
      {
        EmployeeId = "EMP001",
        DeductionId = "COM001",
        AmountOrPercentage = 1000
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

      Deduction deduction = new()
      {
        DeductionId = "COM001",
        CompanyId = "SINGULAR001",
        DeductionType = "DEDUC",
        ShortDescription = "Deduction",
        LongDescription = "Long description for deduction",
        InputType = DeductionInputType.Amount,
        MinimumValue = null,
        MaximumValue = null,
        EmployerContributed = false,
        Status = true,
        ModifiedDate = DateTime.UtcNow,
      };

      EmployeeDeduction employeeDeduction = new()
      {
        EmployeeId = employeeDeductionAddDto.EmployeeId,
        DeductionId = employeeDeductionAddDto.DeductionId,
        DeductionType = deduction.DeductionType,
        DeductionInputType = deduction.InputType,
        AmountOrPercentage = employeeDeductionAddDto.AmountOrPercentage,
        CalculatedDeductionAmount = employeeDeductionAddDto.AmountOrPercentage,
        PayrollRunId = currentPayrollRun.PayrollRunId,
        IsLocked = false
      };


      _ = _payrollRunRepositoryMock
        .Setup(r => r.GetCurrentRunAsync())
        .ReturnsAsync(new PayrollRun { PayrollRunId = 1 });

      _ = _employeeRepositoryMock
        .Setup(r => r.GetEmployeeByIdAsync(fakeEmployee.EmployeeId))
        .ReturnsAsync(fakeEmployee);

      _ = _employeeDeductionRepositoryMock
        .Setup(r => r.CheckIfEmployeeDeductionExistsForCurrentPayrun(fakeEmployee.EmployeeId, deduction.DeductionId, currentPayrollRun.PayrollRunId));

      _ = _deductionRepositoryMock
        .Setup(r => r.GetDeductionByCodeAsync(deduction.DeductionId))
        .ReturnsAsync(deduction);

      _ = _employeeDeductionRepositoryMock
        .Setup(r => r.AddAsync(It.IsAny<EmployeeDeduction>()))
        .ReturnsAsync(employeeDeduction);

      //Act
      EmployeeDeductionDto result = await _employeeDeductionServiceMock.AddAsync(employeeDeductionAddDto);

      //Assert
      _ = Assert.IsType<EmployeeDeductionDto>(result);
      Assert.Equal(employeeDeduction.EmployeeId, result.EmployeeId);
      Assert.Equal(employeeDeduction.DeductionId, result.DeductionId);
      Assert.Equal(employeeDeduction.DeductionType, result.DeductionType);
      Assert.Equal(employeeDeduction.AmountOrPercentage, result.AmountOrPercentage);
      Assert.Equal(employeeDeduction.CalculatedDeductionAmount, result.CalculatedDeductionAmount);
      Assert.Equal(employeeDeduction.PayrollRunId, result.PayRunId);
      Assert.Equal(employeeDeduction.IsLocked, result.IsLocked);
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllEmployeeDeductions()
    {
      //Arrange
      List<EmployeeDeduction> dummyDeductions = new()
      {
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED001", DeductionType = "PAYE", IsLocked = false, PayrollRunId = 100 },
        new EmployeeDeduction { EmployeeId = "EMP002", DeductionId = "DED002", DeductionType = "UIF", IsLocked = false, PayrollRunId = 101 },
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED003", DeductionType = "Other",IsLocked = false, PayrollRunId = 100 }
      };

      _ = _employeeDeductionRepositoryMock
        .Setup(r => r.GetAllAsync())
        .ReturnsAsync(dummyDeductions);

      //Act
      List<EmployeeDeductionDto> result = await _employeeDeductionServiceMock.GetAllAsync();

      //Assert
      _ = Assert.IsType<List<EmployeeDeductionDto>>(result);
      Assert.Equal(3, result.Count);
      Assert.Equal(dummyDeductions[0].EmployeeId, result[0].EmployeeId);
      Assert.Equal(dummyDeductions[1].EmployeeId, result[1].EmployeeId);
      Assert.Equal(dummyDeductions[2].EmployeeId, result[2].EmployeeId);
    }

    [Fact]
    public async Task GetByEmployeeIdAsyncReturnsEmployeeDeductions()
    {
      //Arrange
      string employeeeId = "EMP001";
      List<EmployeeDeduction> employeeDeductions = new()
      {
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED001", DeductionType = "PAYE", IsLocked = true, PayrollRunId = 100 },
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED002", DeductionType = "UIF", IsLocked = true, PayrollRunId = 100 },
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED001", DeductionType = "UIF",IsLocked = false, PayrollRunId = 101 }
      };

      _ = _employeeDeductionRepositoryMock
        .Setup(r => r.GetByEmployeeIdAsync(employeeeId))
        .ReturnsAsync(employeeDeductions);

      //Act
      List<EmployeeDeductionDto> result = await _employeeDeductionServiceMock.GetByEmployeeIdAsync(employeeeId);

      //Assert
      _ = Assert.IsType<List<EmployeeDeductionDto>>(result);
      Assert.Equal(3, result.Count);
      Assert.All(result, ed => Assert.Equal(employeeeId, ed.EmployeeId));
    }

    [Fact]
    public async Task GetByEmployeeIdAndIsNotLockedAsyncReturnsEmployeeDeductionsNotLocked()
    {
      //Arrange
      string employeeeId = "EMP001";
      List<EmployeeDeduction> employeeDeductions = new()
      {
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED001", DeductionType = "PAYE", IsLocked = true, PayrollRunId = 100 },
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED002", DeductionType = "UIF", IsLocked = true, PayrollRunId = 100 },
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED001", DeductionType = "UIF",IsLocked = false, PayrollRunId = 101 }
      };

      _ = _employeeDeductionRepositoryMock
        .Setup(r => r.GetByEmployeeIdAndIsNotLockedAsync(employeeeId))
        .ReturnsAsync(employeeDeductions.Where(ed => !ed.IsLocked).ToList());

      //Act
      List<EmployeeDeductionDto> result = await _employeeDeductionServiceMock.GetByEmployeeIdAndIsNotLockedAsync(employeeeId);

      //Assert
      _ = Assert.IsType<List<EmployeeDeductionDto>>(result);
      _ = Assert.Single(result);
      Assert.All(result, ed => Assert.Equal(employeeeId, ed.EmployeeId));
      Assert.All(result, ed => Assert.False(ed.IsLocked));
    }

    [Fact]
    public async Task GetByEmployeeIdAndLastRunIdAsync()
    {
      //Arrange
      string employeeeId = "EMP001";
      int currentPayrollRunId = 101;
      List<EmployeeDeduction> employeeDeductions = new()
      {
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED001", DeductionType = "PAYE", IsLocked = true, PayrollRunId = 100 },
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED002", DeductionType = "UIF", IsLocked = true, PayrollRunId = 100 },
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED001", DeductionType = "UIF",IsLocked = false, PayrollRunId = 101 }
      };



      _ = _employeeDeductionRepositoryMock
        .Setup(r => r.GetByEmployeeIdAndLastRunIdAsync(employeeeId))
        .ReturnsAsync(employeeDeductions.Where(ed => ed.PayrollRunId == currentPayrollRunId).ToList());

      //Act
      List<EmployeeDeductionDto> result = await _employeeDeductionServiceMock.GetByEmployeeIdAndLastRunIdAsync(employeeeId);

      //Assert
      _ = Assert.IsType<List<EmployeeDeductionDto>>(result);
      _ = Assert.Single(result);
      Assert.All(result, ed => Assert.Equal(currentPayrollRunId, ed.PayRunId));
    }


    [Fact]
    public async Task GetByPayrollRunIdAsyncReturnEmployeeDeductionsForCurrentPayrollrun()
    {
      //Arrange
      int currentPayrollRunId = 101;
      List<EmployeeDeduction> employeeDeductions = new()
      {
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED001", DeductionType = "PAYE", IsLocked = true, PayrollRunId = 100 },
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED002", DeductionType = "UIF", IsLocked = true, PayrollRunId = 100 },
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED001", DeductionType = "UIF",IsLocked = false, PayrollRunId = 101 }
      };

      _ = _employeeDeductionRepositoryMock
        .Setup(r => r.GetByPayrollRunIdAsync(currentPayrollRunId))
        .ReturnsAsync(employeeDeductions.Where(ed => ed.PayrollRunId == currentPayrollRunId).ToList());

      //Act
      List<EmployeeDeductionDto> result = await _employeeDeductionServiceMock.GetByPayrollRunIdAsync(currentPayrollRunId);

      //Assert
      _ = Assert.IsType<List<EmployeeDeductionDto>>(result);
      _ = Assert.Single(result);
      Assert.All(result, ed => Assert.Equal(currentPayrollRunId, ed.PayRunId));
    }

    [Fact]
    public async Task GetByDeductionIdAsyncReturnsEmployeeDeductionWithSpecifiedDeduction()
    {
      //Arrange
      string deductionCode = "DED001";
      List<EmployeeDeduction> employeeDeductions = new()
      {
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED001", DeductionType = "PAYE", IsLocked = true, PayrollRunId = 100 },
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED002", DeductionType = "UIF", IsLocked = true, PayrollRunId = 100 },
        new EmployeeDeduction { EmployeeId = "EMP001", DeductionId = "DED001", DeductionType = "UIF",IsLocked = false, PayrollRunId = 101 }
      };

      _ = _employeeDeductionRepositoryMock
        .Setup(r => r.GetByDeductionIdAsync(deductionCode))
        .ReturnsAsync(employeeDeductions.Where(ed => ed.DeductionId == deductionCode).ToList());

      //Act
      List<EmployeeDeductionDto> result = await _employeeDeductionServiceMock.GetByDeductionIdAsync(deductionCode);

      //Assert
      _ = Assert.IsType<List<EmployeeDeductionDto>>(result);
      Assert.Equal(2, result.Count);
      Assert.All(result, ed => Assert.Equal(deductionCode, ed.DeductionId));
    }

    [Fact]
    public async Task UpdateAsyncUpdatedEmployeeDeduction()
    {
      //Arrange
      PayrollRun currentPayrollRun = new()
      {
        PayrollRunId = 1
      };

      EmployeeDeductionUpdateDto employeeDeductionUpdateDto = new()
      {
        EmployeeId = "EMP001",
        DeductionId = "DED001",
        AmountOrPercentage = 100
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

      Deduction deduction = new()
      {
        DeductionId = "DED001",
        CompanyId = "SINGULAR001",
        DeductionType = "DEDUC",
        ShortDescription = "Deduction",
        LongDescription = "Long description for deduction",
        InputType = DeductionInputType.Amount,
        MinimumValue = null,
        MaximumValue = null,
        EmployerContributed = false,
        Status = true,
        ModifiedDate = DateTime.UtcNow,
      };

      EmployeeDeduction employeeDeduction = new()
      {
        EmployeeId = employeeDeductionUpdateDto.EmployeeId,
        DeductionId = employeeDeductionUpdateDto.DeductionId,
        DeductionType = deduction.DeductionType,
        DeductionInputType = deduction.InputType,
        AmountOrPercentage = employeeDeductionUpdateDto.AmountOrPercentage,
        CalculatedDeductionAmount = employeeDeductionUpdateDto.AmountOrPercentage,
        PayrollRunId = currentPayrollRun.PayrollRunId,
        IsLocked = false
      };


      _ = _payrollRunRepositoryMock
        .Setup(r => r.GetCurrentRunAsync())
        .ReturnsAsync(currentPayrollRun);

      _ = _employeeRepositoryMock
        .Setup(r => r.GetEmployeeByIdAsync(fakeEmployee.EmployeeId))
        .ReturnsAsync(fakeEmployee);

      _ = _employeeDeductionRepositoryMock
        .Setup(r => r.CheckIfEmployeeDeductionExistsForCurrentPayrun(fakeEmployee.EmployeeId, deduction.DeductionId, currentPayrollRun.PayrollRunId))
        .ReturnsAsync(employeeDeduction);

      _ = _deductionRepositoryMock
        .Setup(r => r.GetDeductionByCodeAsync(deduction.DeductionId))
        .ReturnsAsync(deduction);

      _ = _employeeDeductionRepositoryMock
        .Setup(r => r.UpdateAsync(employeeDeduction))
        .ReturnsAsync(employeeDeduction);

      //Act
      EmployeeDeductionDto result = await _employeeDeductionServiceMock.UpdateAsync(employeeDeductionUpdateDto);

      //Assert
      _ = Assert.IsType<EmployeeDeductionDto>(result);
      Assert.Equal(employeeDeductionUpdateDto.EmployeeId, result.EmployeeId);
      Assert.Equal(employeeDeductionUpdateDto.DeductionId, result.DeductionId);
    }
  }
}

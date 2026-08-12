namespace HRConnect.Tests
{
  using HRConnect.Api.DTOs.Payroll.Deduction;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Services;
  using HRConnect.Api.Utils;
  using Moq;

  public class DeductionServiceTests
  {
    private readonly DeductionService _deductionService;
    private readonly Mock<IDeductionRepository> _deductionRepositoryMock;
    private readonly Mock<IEmployeeDeductionRepository> _employeeDeductionRepositoryMock;
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IPayrollRunRepository> _payrollRunRepositoryMock;

    public DeductionServiceTests()
    {
      _deductionRepositoryMock = new Mock<IDeductionRepository>();
      _employeeDeductionRepositoryMock = new Mock<IEmployeeDeductionRepository>();
      _employeeRepositoryMock = new Mock<IEmployeeRepository>();
      _payrollRunRepositoryMock = new Mock<IPayrollRunRepository>();
      _deductionService = new DeductionService(
        _deductionRepositoryMock.Object,
        _employeeDeductionRepositoryMock.Object,
        _employeeRepositoryMock.Object,
        _payrollRunRepositoryMock.Object);
    }

    [Fact]
    public async Task AddAsyncReturnsAddedDeduction()
    {
      //Arrange
      DeductionAddDto deductionAddDto = new()
      {
        CompanyId = "ABC123",
        ShortDescription = "Test Deduction",
        LongDescription = "This is a test deduction.",
        DeductionType = "Test Deduction Type",
        InputType = DeductionInputType.Amount,
        EmployerContributed = false
      };

      List<string> existingDeductionCodes = ["ABC001", "ABC002", "SIS001"];

      string generatedCode = GenerateUnqiueCode.GenerateStringCode(deductionAddDto.CompanyId[..3], existingDeductionCodes);

      Deduction deduction = new()
      {
        DeductionId = generatedCode,
        CompanyId = deductionAddDto.CompanyId,
        ShortDescription = deductionAddDto.ShortDescription,
        LongDescription = deductionAddDto.LongDescription,
        DeductionType = deductionAddDto.DeductionType,
        InputType = deductionAddDto.InputType,
        MinimumValue = deductionAddDto.MinimumValue,
        MaximumValue = deductionAddDto.MaximumValue,
        EmployerContributed = deductionAddDto.EmployerContributed,
        Status = true,
        ModifiedDate = DateTime.UtcNow
      };

      DeductionDto expectedDeductionDto = new()
      {
        DeductionId = generatedCode,
        CompanyId = deductionAddDto.CompanyId,
        ShortDescription = deductionAddDto.ShortDescription,
        LongDescription = deductionAddDto.LongDescription,
        DeductionType = deductionAddDto.DeductionType,
        InputType = deductionAddDto.InputType,
        MinimumValue = deductionAddDto.MinimumValue,
        MaximumValue = deductionAddDto.MaximumValue,
        EmployerContributed = deductionAddDto.EmployerContributed,
        Status = true,
        ModifiedDate = DateTime.UtcNow
      };

      _ = _deductionRepositoryMock
        .Setup(r => r.GetAllDeductionCodesAsync(deductionAddDto.CompanyId.Substring(0, 3)))
        .ReturnsAsync(existingDeductionCodes);

      _ = _deductionRepositoryMock
        .Setup(r => r.CheckIfDescriptionsExists(deductionAddDto.ShortDescription, deductionAddDto.LongDescription))
        .ReturnsAsync(false);

      _ = _deductionRepositoryMock
        .Setup(r => r.AddAsync(It.IsAny<Deduction>()))
        .ReturnsAsync(deduction);

      //Act
      DeductionDto result = await _deductionService.AddAsync(deductionAddDto);

      //Assert
      _ = Assert.IsType<DeductionDto>(result);
      Assert.NotNull(result);
      Assert.Equal(expectedDeductionDto.CompanyId, result.CompanyId);
      Assert.Equal("ABC003", result.DeductionId);
      Assert.Equal(expectedDeductionDto.ShortDescription, result.ShortDescription);
      Assert.Equal(expectedDeductionDto.LongDescription, result.LongDescription);
      Assert.Equal(expectedDeductionDto.DeductionType, result.DeductionType);
      Assert.Equal(expectedDeductionDto.InputType, result.InputType);
      Assert.Null(result.MinimumValue);
      Assert.Null(result.MaximumValue);
      Assert.Equal(expectedDeductionDto.EmployerContributed, result.EmployerContributed);
      Assert.True(result.Status);
    }

    [Fact]
    public async Task GetAllDeductionsAsyncReturnsAListOfDeductions()
    {
      //Arrange
      List<Deduction> deductions = new()
      {
        new Deduction
        {
          DeductionId = "ABC001",
          CompanyId = "ABC123",
          ShortDescription = "Test Deduction 1",
          LongDescription = "This is the first test deduction.",
          DeductionType = "Test Deduction Type",
          InputType = DeductionInputType.Amount,
          MinimumValue = 10,
          MaximumValue = 100,
          EmployerContributed = false,
          Status = true,
          ModifiedDate = DateTime.UtcNow
        },
        new Deduction
        {
          DeductionId = "ABC002",
          CompanyId = "ABC123",
          ShortDescription = "Test Deduction 2",
          LongDescription = "This is the second test deduction.",
          DeductionType = "Test Deduction Type",
          InputType = DeductionInputType.Percentage,
          MinimumValue = 5,
          MaximumValue = 50,
          EmployerContributed = true,
          Status = true,
          ModifiedDate = DateTime.UtcNow
        }
      };
      _ = _deductionRepositoryMock
        .Setup(r => r.GetAllDeductionsAsync())
        .ReturnsAsync(deductions);

      //Act
      List<DeductionDto> result = await _deductionService.GetAllDeductionsAsync();

      //Assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      Assert.Equal(deductions[0].DeductionId, result[0].DeductionId);
      Assert.Equal(deductions[1].DeductionId, result[1].DeductionId);
    }

    [Fact]
    public async Task GetDeductionsByCompanyIdAsyncReturnsAListOfDeductionsForTheCompany()
    {
      //Arrange
      string companyId = "ABC123";
      List<Deduction> deductions = new()
      {
        new Deduction
        {
          DeductionId = "ABC001",
          CompanyId = companyId,
          ShortDescription = "Test Deduction 1",
          LongDescription = "This is the first test deduction.",
          DeductionType = "Test Deduction Type",
          InputType = DeductionInputType.Amount,
          MinimumValue = 10,
          MaximumValue = 100,
          EmployerContributed = false,
          Status = true,
          ModifiedDate = DateTime.UtcNow
        },
        new Deduction
        {
          DeductionId = "ABC002",
          CompanyId = companyId,
          ShortDescription = "Test Deduction 2",
          LongDescription = "This is the second test deduction.",
          DeductionType = "Test Deduction Type",
          InputType = DeductionInputType.Percentage,
          MinimumValue = 5,
          MaximumValue = 50,
          EmployerContributed = true,
          Status = true,
          ModifiedDate = DateTime.UtcNow
        }
      };
      _ = _deductionRepositoryMock
        .Setup(r => r.GetDeductionByCompanyIdAsync(companyId))
        .ReturnsAsync(deductions);

      //Act
      List<DeductionDto> result = await _deductionService.GetDeductionsByCompanyIdAsync(companyId);

      //Assert
      _ = Assert.IsType<List<DeductionDto>>(result);
      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      Assert.All(result, d => Assert.Equal(companyId, d.CompanyId));
    }

    [Fact]
    public async Task GetDeductionByCodeAsyncReturnsDeductionWithTheGivenCode()
    {
      //Arrange
      string code = "ABC001";
      Deduction deduction = new()
      {
        DeductionId = code,
        CompanyId = "ABC123",
        ShortDescription = "Test Deduction",
        LongDescription = "This is a test deduction.",
        DeductionType = "Test Deduction Type",
        InputType = DeductionInputType.Amount,
        MinimumValue = 10,
        MaximumValue = 100,
        EmployerContributed = false,
        Status = true,
        ModifiedDate = DateTime.UtcNow
      };
      _ = _deductionRepositoryMock
        .Setup(r => r.GetDeductionByCodeAsync(code))
        .ReturnsAsync(deduction);

      //Act
      DeductionDto? result = await _deductionService.GetDeductionByCodeAsync(code);

      //Assert
      _ = Assert.IsType<DeductionDto?>(result);
      Assert.Equal(code, result.DeductionId);
    }

    [Fact]
    public async Task UpdateAsyncReturnsUpdatedDeduction()
    {
      //Arrange
      string code = "ABC001";
      DeductionUpdateDto deductionUpdateDto = new()
      {
        DeductionId = code,
        ShortDescription = "Updated Test Deduction",
        LongDescription = "This is an updated test deduction.",
        DeductionType = "Updated Test Deduction Type",
        InputType = DeductionInputType.Percentage,
        MinimumValue = 5,
        MaximumValue = 50,
        EmployerContributed = true
      };

      Deduction existingDeduction = new()
      {
        DeductionId = code,
        CompanyId = "ABC123",
        ShortDescription = "Test Deduction",
        LongDescription = "This is a test deduction.",
        DeductionType = "Test Deduction Type",
        InputType = DeductionInputType.Amount,
        MinimumValue = 10,
        MaximumValue = 100,
        EmployerContributed = false,
        Status = true,
        ModifiedDate = DateTime.UtcNow
      };

      Deduction updatedDeduction = new()
      {
        DeductionId = code,
        CompanyId = "ABC123",
        ShortDescription = deductionUpdateDto.ShortDescription,
        LongDescription = deductionUpdateDto.LongDescription,
        DeductionType = deductionUpdateDto.DeductionType,
        InputType = DeductionInputType.Percentage,
        MinimumValue = deductionUpdateDto.MinimumValue,
        MaximumValue = deductionUpdateDto.MaximumValue,
        EmployerContributed = (bool)deductionUpdateDto.EmployerContributed,
        Status = true,
        ModifiedDate = DateTime.UtcNow
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

      PayrollRun payrollRun = new()
      {
        PayrollRunId = 1
      };


      List<EmployeeDeduction> employeeDeductions = new()
      {
        new EmployeeDeduction()
        {
          EmployeeId = fakeEmployee.EmployeeId,
          DeductionId = updatedDeduction.DeductionId,
          DeductionType = updatedDeduction.DeductionType,
          DeductionInputType = updatedDeduction.InputType,
          AmountOrPercentage = 1000,
          CalculatedDeductionAmount = 980,
          PayrollRunId = payrollRun.PayrollRunId,
          IsLocked = false
        },
        new EmployeeDeduction()
        {
          EmployeeId = fakeEmployee.EmployeeId,
          DeductionId = updatedDeduction.DeductionId,
          DeductionType = updatedDeduction.DeductionType,
          DeductionInputType = updatedDeduction.InputType,
          AmountOrPercentage = 1000,
          CalculatedDeductionAmount = 980,
          PayrollRunId = payrollRun.PayrollRunId,
          IsLocked = false
        }

      };

      _ = _deductionRepositoryMock
       .Setup(r => r.CheckIfDescriptionsExists(deductionUpdateDto.ShortDescription, deductionUpdateDto.LongDescription))
       .ReturnsAsync(false);

      _ = _deductionRepositoryMock
        .Setup(r => r.GetDeductionByCodeAsync(code))
        .ReturnsAsync(existingDeduction);

      _ = _payrollRunRepositoryMock
        .Setup(r => r.GetCurrentRunAsync())
        .ReturnsAsync(payrollRun);

      _ = _employeeDeductionRepositoryMock
        .Setup(r => r.GetByPayrollRunIdAsync(payrollRun.PayrollRunId))
        .ReturnsAsync(employeeDeductions);

      _ = _deductionRepositoryMock
        .Setup(r => r.UpdateAsync(existingDeduction))
        .ReturnsAsync(updatedDeduction);

      //Act
      DeductionDto result = await _deductionService.UpdateAsync(deductionUpdateDto);

      //Assert
      _ = Assert.IsType<DeductionDto>(result);
      Assert.Equal(code, result.DeductionId);
      Assert.Equal(deductionUpdateDto.ShortDescription, result.ShortDescription);
      Assert.Equal(deductionUpdateDto.LongDescription, result.LongDescription);
      Assert.Equal(deductionUpdateDto.DeductionType, result.DeductionType);
      Assert.Equal(deductionUpdateDto.InputType, result.InputType);
      Assert.Equal(deductionUpdateDto.MinimumValue, result.MinimumValue);
      Assert.Equal(deductionUpdateDto.MaximumValue, result.MaximumValue);
      Assert.Equal(deductionUpdateDto.EmployerContributed, result.EmployerContributed);
    }

    [Fact]
    public async Task DeleteAsyncReturnsMessageIndicatingSuccessfulDeletion()
    {
      //Arrange
      string code = "ABC001";
      _ = _deductionRepositoryMock
        .Setup(r => r.DeleteAsync(code))
        .ReturnsAsync($"Deduction with code {code} has been successfully deleted.");

      //Act
      string result = await _deductionService.DeleteAsync(code);

      //Assert
      _ = Assert.IsType<string>(result);
      Assert.Equal($"Deduction with code {code} has been successfully deleted.", result);
    }
  }
}

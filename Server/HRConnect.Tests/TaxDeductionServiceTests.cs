namespace HRConnect.Tests
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Models;
  using HRConnect.Api.Services;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Repositories;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.DTOs.TaxDeduction;
  using Moq;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Xunit;

  public class TaxDeductionServiceTests
  {
    private Mock<ITaxDeductionRepository> _mockRepository;
    private TaxDeductionService _service;

    private List<TaxTableUpload> _taxTableUploads;
    private List<TaxDeduction> _taxDeductions;

    // Shared test data for GenerateTaxAsync tests
    private Employee _employee;
    private PensionDeduction _pension;
    private PayrollRun _payrollRun;

    public TaxDeductionServiceTests()
    {
      _mockRepository = new Mock<ITaxDeductionRepository>();

      _taxTableUploads = new List<TaxTableUpload>
      {
          new TaxTableUpload
          {
              TaxYear = 2026,
              EffectiveFrom = DateTime.UtcNow.AddDays(-1),
              EffectiveTo = null
          }
      };

      _taxDeductions = new List<TaxDeduction>
      {
          new TaxDeduction
          {
              Id = 1,
              TaxYear = 2026,
              Remuneration = 10000,
              AnnualEquivalent = 120000,
              TaxUnder65 = 1000,
              Tax65To74 = 800,
              TaxOver75 = 600
          },
          new TaxDeduction
          {
              Id = 2,
              TaxYear = 2026,
              Remuneration = 20000,
              AnnualEquivalent = 240000,
              TaxUnder65 = 2000,
              Tax65To74 = 1600,
              TaxOver75 = 1200
          }
      };

      // Shared employee for GenerateTaxAsync tests
      _employee = new Employee
      {
        EmployeeId = "EMP001",
        Name = "Test",
        Surname = "Employee",
        Email = "test@company.com",
        IdNumber = "0201014515088",
        DateOfBirth = new DateOnly(2002, 1, 1), // age 24
        MonthlySalary = 15000m
      };

      // Shared pension for GenerateTaxAsync tests
      _pension = new PensionDeduction
      {
        EmployeeId = "EMP001",
        PensionableSalary = 15000m,
        PendsionCategoryPercentage = 5m,
        PensionContribution = 750m,
        VoluntaryContribution = 0m,
        TotalPensionContribution = 750m
      };

      // Shared payroll run
      _payrollRun = new PayrollRun
      {
        PayrollRunId = 1,
        IsLocked = false,
        IsFinalised = false
      };

      // Always return some list to avoid null
      _mockRepository.Setup(r => r.GetActiveTaxTableUploadsAsync())
          .ReturnsAsync(_taxTableUploads);

      _mockRepository.Setup(r => r.GetTaxDeductionsByYearAsync(It.IsAny<int>()))
          .ReturnsAsync((int year) => year == 2026 ? _taxDeductions : new List<TaxDeduction>());

      _mockRepository.Setup(r => r.SaveChangesAsync())
          .Returns(Task.CompletedTask);

      _mockRepository.Setup(r => r.GetEmployeeByEmailAsync("test@company.com"))
          .ReturnsAsync(_employee);

      _mockRepository.Setup(r => r.GetActivePayrollRunAsync())
          .ReturnsAsync(_payrollRun);

      _mockRepository.Setup(r => r.GetPensionByEmployeeIdAsync("EMP001"))
          .ReturnsAsync(_pension);

      _mockRepository.Setup(r => r.GetExistingFinalTaxAsync(It.IsAny<string>(), It.IsAny<int>()))
          .ReturnsAsync((FinalTaxDeduction?)null);

      _mockRepository.Setup(r => r.AddFinalTaxDeductionAsync(It.IsAny<FinalTaxDeduction>()))
          .Returns(Task.CompletedTask);

      _service = new TaxDeductionService(_mockRepository.Object);
    }

    [Fact]
    public async Task CalculateTaxAsyncReturnsCorrectTaxFromTable()
    {
      var tax = await _service.CalculateTaxAsync(15000, 30);
      Assert.Equal(2000, tax); // Falls in 2nd bracket
    }

    [Fact]
    public async Task CalculateTaxAsyncHighEarnerUnder65()
    {
      decimal highSalary = 500000;

      decimal monthlyRem = Math.Max(0, highSalary / 12 - 156_328 / 12);
      decimal expectedTax = Math.Floor(54481 + 0.45m * monthlyRem);

      var tax = await _service.CalculateTaxAsync(highSalary, 30);
      Assert.Equal(expectedTax, tax);
    }

    [Fact]
    public async Task CalculateTaxAsyncHighEarnerAge65To74()
    {
      decimal highSalary = 500000;

      decimal monthlyRem = Math.Max(0, highSalary / 12 - 156_328 / 12);
      decimal expectedTax = Math.Floor(53694 + 0.45m * monthlyRem);

      var tax = await _service.CalculateTaxAsync(highSalary, 70);
      Assert.Equal(expectedTax, tax);
    }

    [Fact]
    public async Task CalculateTaxAsyncHighEarnerAgeOver75()
    {
      decimal highSalary = 500000;

       decimal monthlyRem = Math.Max(0, highSalary / 12 - 156_328 / 12);
      decimal expectedTax = Math.Floor(53694 + 0.45m * monthlyRem);

      var tax = await _service.CalculateTaxAsync(highSalary, 80);
      Assert.Equal(expectedTax, tax);
    }

    [Fact]
    public async Task GetAllTaxDeductionsAsyncReturnsAllRecords()
    {
      var deductions = await _service.GetAllTaxDeductionsAsync(2026);
      Assert.Equal(2, deductions.Count);
    }

    [Fact]
    public async Task UpdateTaxDeductionAsyncThrowsWhenTaxYearChanged()
    {
      var dto = new UpdateTaxDeductionDto
      {
        Id = 1,
        TaxYear = 2026,
        Remuneration = 10000,
        AnnualEquivalent = 120000,
        TaxUnder65 = 1000,
        Tax65To74 = 800,
        TaxOver75 = 600
      };

      dto.TaxYear = 2027;

      _mockRepository.Setup(r => r.GetTaxDeductionsByYearAsync(2027))
          .ReturnsAsync(_taxDeductions);

      await Assert.ThrowsAsync<InvalidOperationException>(() =>
          _service.UpdateTaxDeductionAsync(dto));
    }

    [Fact]
    public async Task UpdateTaxDeductionAsyncSuccessfullyUpdatesValues()
    {
      var dto = new UpdateTaxDeductionDto
      {
        Id = 1,
        TaxYear = 2026,
        Remuneration = 15000,
        AnnualEquivalent = 180000,
        TaxUnder65 = 1500,
        Tax65To74 = 1200,
        TaxOver75 = 1000
      };

      await _service.UpdateTaxDeductionAsync(dto);

      var updated = _taxDeductions.First(x => x.Id == 1);

      Assert.Equal(15000, updated.Remuneration);
      Assert.Equal(180000, updated.AnnualEquivalent);
      Assert.Equal(1500, updated.TaxUnder65);
      Assert.Equal(1200, updated.Tax65To74);
      Assert.Equal(1000, updated.TaxOver75);
    }

    [Fact]
    public async Task UpdateTaxDeductionAsyncUpdatesValuesCorrectly()
    {
      var dto = new UpdateTaxDeductionDto
      {
        Id = 1,
        TaxYear = 2026,
        Remuneration = 11000,
        AnnualEquivalent = 130000,
        TaxUnder65 = 1100,
        Tax65To74 = 900,
        TaxOver75 = 700
      };

      await _service.UpdateTaxDeductionAsync(dto);

      var updatedEntity = _taxDeductions.First(x => x.Id == 1);
      Assert.Equal(11000, updatedEntity.Remuneration);
      Assert.Equal(130000, updatedEntity.AnnualEquivalent);
      Assert.Equal(1100, updatedEntity.TaxUnder65);
      Assert.Equal(900, updatedEntity.Tax65To74);
      Assert.Equal(700, updatedEntity.TaxOver75);
    }

    // ─── GenerateTaxAsync Tests ───────────────────────────────────────────────

    [Fact]
    public async Task GenerateTaxAsync_ReturnsRecord_WithCorrectTaxableIncome()
    {
      var request = new TaxCalculationDto
      {
        MedicalAidMembers = 0,
        MedicalAidDependants = 0,
        MedicalAidChildren = 0
      };

      var result = await _service.GenerateTaxAsync(request, "test@company.com");

      // taxableIncome = monthlySalary - totalPensionContribution = 15000 - 750 = 14250
      Assert.Equal(14250m, result.PensionableIncome);
    }

    [Fact]
    public async Task GenerateTaxAsync_ReturnsRecord_WithCorrectPensionContribution()
    {
      var request = new TaxCalculationDto
      {
        MedicalAidMembers = 0,
        MedicalAidDependants = 0,
        MedicalAidChildren = 0
      };

      var result = await _service.GenerateTaxAsync(request, "test@company.com");

      Assert.Equal(750m, result.PensionContribution);
    }

    [Fact]
    public async Task GenerateTaxAsync_NoMedicalAid_AppliesZeroMedicalCredit()
    {
      var request = new TaxCalculationDto
      {
        MedicalAidMembers = 0,
        MedicalAidDependants = 0,
        MedicalAidChildren = 0
      };

      var result = await _service.GenerateTaxAsync(request, "test@company.com");

      Assert.Equal(0m, result.MedicalTaxCredit);
    }

    [Fact]
    public async Task GenerateTaxAsync_OneMember_AppliesCorrectMedicalCredit()
    {
      var request = new TaxCalculationDto
      {
        MedicalAidMembers = 1,
        MedicalAidDependants = 0,
        MedicalAidChildren = 0
      };

      var result = await _service.GenerateTaxAsync(request, "test@company.com");

      // 1 main member = R364
      Assert.Equal(364m, result.MedicalTaxCredit);
    }

    [Fact]
    public async Task GenerateTaxAsync_MembersAndChildren_AppliesCorrectMedicalCredit()
    {
      var request = new TaxCalculationDto
      {
        MedicalAidMembers = 2,
        MedicalAidDependants = 1,
        MedicalAidChildren = 2
      };

      var result = await _service.GenerateTaxAsync(request, "test@company.com");

      // 364 (main) + 1*364 (extra member) + 1*364 (dependant) + 2*246 (children)
      decimal expected = 364m + (1 * 364m) + (1 * 364m) + (2 * 246m);
      Assert.Equal(expected, result.MedicalTaxCredit);
    }

    [Fact]
    public async Task GenerateTaxAsync_FinalTax_IsNeverNegative()
    {
      // Give massive medical credits — finalTax should floor at 0
      var request = new TaxCalculationDto
      {
        MedicalAidMembers = 100,
        MedicalAidDependants = 0,
        MedicalAidChildren = 0
      };

      var result = await _service.GenerateTaxAsync(request, "test@company.com");

      Assert.True(result.TaxDeductionAmount >= 0);
    }

    [Fact]
    public async Task GenerateTaxAsync_ThrowsKeyNotFoundException_WhenEmployeeNotFound()
    {
      _mockRepository.Setup(r => r.GetEmployeeByEmailAsync("unknown@company.com"))
          .ReturnsAsync((Employee?)null);

      var request = new TaxCalculationDto
      {
        MedicalAidMembers = 1,
        MedicalAidDependants = 0,
        MedicalAidChildren = 0
      };

      await Assert.ThrowsAsync<KeyNotFoundException>(() =>
          _service.GenerateTaxAsync(request, "unknown@company.com"));
    }

    [Fact]
    public async Task GenerateTaxAsync_ThrowsKeyNotFoundException_WhenNoActivePayrollRun()
    {
      _mockRepository.Setup(r => r.GetActivePayrollRunAsync())
          .ReturnsAsync((PayrollRun?)null);

      var request = new TaxCalculationDto
      {
        MedicalAidMembers = 1,
        MedicalAidDependants = 0,
        MedicalAidChildren = 0
      };

      await Assert.ThrowsAsync<KeyNotFoundException>(() =>
          _service.GenerateTaxAsync(request, "test@company.com"));
    }

    [Fact]
    public async Task GenerateTaxAsync_ThrowsInvalidOperation_WhenPayrollIsLocked()
    {
      _mockRepository.Setup(r => r.GetExistingFinalTaxAsync(It.IsAny<string>(), It.IsAny<int>()))
          .ReturnsAsync(new FinalTaxDeduction { IsLocked = true });

      var request = new TaxCalculationDto
      {
        MedicalAidMembers = 1,
        MedicalAidDependants = 0,
        MedicalAidChildren = 0
      };

      await Assert.ThrowsAsync<InvalidOperationException>(() =>
          _service.GenerateTaxAsync(request, "test@company.com"));
    }

    [Fact]
    public async Task GenerateTaxAsync_ThrowsInvalidOperation_WhenPayrollRunIsFinalised()
    {
      _mockRepository.Setup(r => r.GetActivePayrollRunAsync())
          .ReturnsAsync(new PayrollRun { PayrollRunId = 1, IsLocked = false, IsFinalised = true });

      var request = new TaxCalculationDto
      {
        MedicalAidMembers = 1,
        MedicalAidDependants = 0,
        MedicalAidChildren = 0
      };

      await Assert.ThrowsAsync<InvalidOperationException>(() =>
          _service.GenerateTaxAsync(request, "test@company.com"));
    }

    [Fact]
    public async Task GenerateTaxAsync_ThrowsKeyNotFoundException_WhenPensionNotFound()
    {
      _mockRepository.Setup(r => r.GetPensionByEmployeeIdAsync("EMP001"))
          .ReturnsAsync((PensionDeduction?)null);

      var request = new TaxCalculationDto
      {
        MedicalAidMembers = 1,
        MedicalAidDependants = 0,
        MedicalAidChildren = 0
      };

      await Assert.ThrowsAsync<KeyNotFoundException>(() =>
          _service.GenerateTaxAsync(request, "test@company.com"));
    }

    [Fact]
    public async Task GenerateTaxAsync_StoresCorrectEmployeeId()
    {
      var request = new TaxCalculationDto
      {
        MedicalAidMembers = 0,
        MedicalAidDependants = 0,
        MedicalAidChildren = 0
      };

      var result = await _service.GenerateTaxAsync(request, "test@company.com");

      Assert.Equal("EMP001", result.EmployeeId);
    }

    [Fact]
    public async Task GenerateTaxAsync_GeneratesCorrectTaxCode()
    {
      var request = new TaxCalculationDto
      {
        MedicalAidMembers = 0,
        MedicalAidDependants = 0,
        MedicalAidChildren = 0
      };

      var result = await _service.GenerateTaxAsync(request, "test@company.com");

      Assert.Equal($"TX-{DateTime.Now.Year}-1-EMP001", result.TaxCode);
    }
  }
}
namespace HRConnect.Tests
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Models;
  using HRConnect.Api.Services;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Repositories;
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

      // Always return some list to avoid null
      _mockRepository.Setup(r => r.GetActiveTaxTableUploadsAsync())
          .ReturnsAsync(_taxTableUploads);

      _mockRepository.Setup(r => r.GetTaxDeductionsByYearAsync(It.IsAny<int>()))
          .ReturnsAsync((int year) => year == 2026 ? _taxDeductions : new List<TaxDeduction>());

      _mockRepository.Setup(r => r.SaveChangesAsync())
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
      var tax = await _service.CalculateTaxAsync(highSalary, 30);

      decimal monthlyRem = highSalary / 12;
      decimal expectedTax = Math.Floor(54481 + 0.45m * (monthlyRem - 156_328 / 12));

      Assert.Equal(expectedTax, tax);
    }

    [Fact]
    public async Task CalculateTaxAsyncHighEarnerAge65To74()
    {
      decimal highSalary = 500000;
      var tax = await _service.CalculateTaxAsync(highSalary, 70);

      decimal monthlyRem = highSalary / 12;
      decimal expectedTax = Math.Floor(53694 + 0.45m * (monthlyRem - 156_328 / 12));

      Assert.Equal(expectedTax, tax);
    }

    [Fact]
    public async Task CalculateTaxAsyncHighEarnerAgeOver75()
    {
      decimal highSalary = 500000;
      var tax = await _service.CalculateTaxAsync(highSalary, 80);

      decimal monthlyRem = highSalary / 12;
      decimal expectedTax = Math.Floor(53432 + 0.45m * (monthlyRem - 156_328 / 12));

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
      // Use existing year 2026
      var dto = new UpdateTaxDeductionDto
      {
        Id = 1,
        TaxYear = 2026, // Existing year
        Remuneration = 10000,
        AnnualEquivalent = 120000,
        TaxUnder65 = 1000,
        Tax65To74 = 800,
        TaxOver75 = 600
      };

      // attempt to change TaxYear inside the service
      dto.TaxYear = 2027; // Attempt to change year (this triggers InvalidOperationException)

      // Setup repository to return the existing deduction for 2027 as well (so list is not null)
      _mockRepository.Setup(r => r.GetTaxDeductionsByYearAsync(2027))
          .ReturnsAsync(_taxDeductions); // reuse same list

      await Assert.ThrowsAsync<InvalidOperationException>(() =>
          _service.UpdateTaxDeductionAsync(dto));
    }

    [Fact]
    public async Task UpdateTaxDeductionAsyncSuccessfullyUpdatesValues()
    {
      // Arrange: pick the first deduction
      var dto = new UpdateTaxDeductionDto
      {
        Id = 1,
        TaxYear = 2026, // Same as the entity
        Remuneration = 15000,
        AnnualEquivalent = 180000,
        TaxUnder65 = 1500,
        Tax65To74 = 1200,
        TaxOver75 = 1000
      };

      // Act: update the deduction
      await _service.UpdateTaxDeductionAsync(dto);

      // Assert: the entity should be updated in the in-memory list
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
  }
}

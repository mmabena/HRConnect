namespace HRConnect.Tests
{
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Models;
  using HRConnect.Api.Services;
  using Microsoft.EntityFrameworkCore;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Xunit;


  public class TaxDeductionServiceTests
  {
    private static ApplicationDBContext GetDbContext()
    {
      var options = new DbContextOptionsBuilder<ApplicationDBContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;

      return new ApplicationDBContext(options);
    }

    [Fact]
    public async Task CalculateTaxAsyncReturnsCorrectTax()
    {
      var context = GetDbContext();

      // active tax table
      context.TaxTableUploads.Add(new TaxTableUpload
      {
        TaxYear = 2026,
        FileName = "2026.xlsx",
        FileUrl = "2026.xlsx",
        EffectiveFrom = DateTime.UtcNow.AddDays(-1),
        EffectiveTo = null
      });

      // tax deductions for that year
      context.TaxDeductions.AddRange(new List<TaxDeduction>
            {
                new TaxDeduction
                {
                    TaxYear = 2026,
                    Remuneration = 10000,
                    AnnualEquivalent = 120000,
                    TaxUnder65 = 1000,
                    Tax65To74 = 800,
                    TaxOver75 = 600
                },
                new TaxDeduction
                {
                    TaxYear = 2026,
                    Remuneration = 20000,
                    AnnualEquivalent = 240000,
                    TaxUnder65 = 2000,
                    Tax65To74 = 1600,
                    TaxOver75 = 1200
                }
            });

      await context.SaveChangesAsync();

      var service = new TaxDeductionService(context);

      // Act
      var tax = await service.CalculateTaxAsync(15000, 30);

      // Assert
      Assert.Equal(2000, tax);
    }

    [Fact]
    public async Task GetAllTaxDeductionsAsyncReturnsAllRecords()
    {
      var context = GetDbContext();

      context.TaxDeductions.AddRange(new List<TaxDeduction>
            {
                new TaxDeduction
                {
                    TaxYear = 2026,
                    Remuneration = 10000,
                    AnnualEquivalent = 120000,
                    TaxUnder65 = 1000,
                    Tax65To74 = 800,
                    TaxOver75 = 600
                },
                new TaxDeduction
                {
                    TaxYear = 2026,
                    Remuneration = 20000,
                    AnnualEquivalent = 240000,
                    TaxUnder65 = 2000,
                    Tax65To74 = 1600,
                    TaxOver75 = 1200
                }
            });

      await context.SaveChangesAsync();

      var service = new TaxDeductionService(context);

      var deductions = await service.GetAllTaxDeductionsAsync(2026);

      Assert.Equal(2, deductions.Count);
    }

    [Fact]
    public async Task UpdateTaxDeductionAsyncThrowsWhenTaxYearChanged()
    {
      var context = GetDbContext();

      var entity = new TaxDeduction
      {
        TaxYear = 2026,
        Remuneration = 10000,
        AnnualEquivalent = 120000,
        TaxUnder65 = 1000,
        Tax65To74 = 800,
        TaxOver75 = 600
      };

      context.TaxDeductions.Add(entity);
      await context.SaveChangesAsync();

      var service = new TaxDeductionService(context);

      var dto = new UpdateTaxDeductionDto
      {
        Id = entity.Id,
        TaxYear = 2027, // Attempt to change year
        Remuneration = 10000,
        AnnualEquivalent = 120000,
        TaxUnder65 = 1000,
        Tax65To74 = 800,
        TaxOver75 = 600
      };

      await Assert.ThrowsAsync<InvalidOperationException>(() =>
          service.UpdateTaxDeductionAsync(dto));
    }

    [Fact]
    public async Task CalculateTaxAsyncReturnsHighEarnerTaxUnder65()
    {
      var context = GetDbContext();

      // Active tax table with upper limit lower than high-earner test
      context.TaxTableUploads.Add(new TaxTableUpload
      {
        TaxYear = 2026,
        FileName = "2026.xlsx",
        FileUrl = "2026.xlsx",
        EffectiveFrom = DateTime.UtcNow.AddDays(-1),
        EffectiveTo = null
      });

      // Tax table only goes up to 20000
      context.TaxDeductions.AddRange(new List<TaxDeduction>
    {
        new TaxDeduction
        {
            TaxYear = 2026,
            Remuneration = 20000,
            AnnualEquivalent = 240000,
            TaxUnder65 = 2000,
            Tax65To74 = 1600,
            TaxOver75 = 1200
        }
    });

      await context.SaveChangesAsync();

      var service = new TaxDeductionService(context);

      // Act: high-earner monthly salary > max in table
      decimal highSalary = 500000; // annual
      var tax = await service.CalculateTaxAsync(highSalary, 30); // under 65

      // Formula: R54,481 + 45% of (monthly remuneration - 156,328/12)
      decimal monthlyRem = highSalary / 12;
      decimal expectedTax = 54481 + 0.45m * (monthlyRem - 156328m / 12);
      expectedTax = Math.Floor(expectedTax);

      // Assert
      Assert.Equal(expectedTax, tax);
    }

    [Fact]
    public async Task CalculateTaxAsyncReturnsHighEarnerTaxAge65To74()
    {
      var context = GetDbContext();

      context.TaxTableUploads.Add(new TaxTableUpload
      {
        TaxYear = 2026,
        FileName = "2026.xlsx",
        FileUrl = "2026.xlsx",
        EffectiveFrom = DateTime.UtcNow.AddDays(-1),
        EffectiveTo = null
      });

      context.TaxDeductions.Add(new TaxDeduction
      {
        TaxYear = 2026,
        Remuneration = 20000,
        AnnualEquivalent = 240000,
        TaxUnder65 = 2000,
        Tax65To74 = 1600,
        TaxOver75 = 1200
      });

      await context.SaveChangesAsync();

      var service = new TaxDeductionService(context);

      decimal highSalary = 500000;
      int age = 70;

      var tax = await service.CalculateTaxAsync(highSalary, age);

      decimal monthlyRem = highSalary / 12;
      decimal expectedTax = 53694 + 0.45m * (monthlyRem - 156328m / 12);
      expectedTax = Math.Floor(expectedTax);

      Assert.Equal(expectedTax, tax);
    }

    [Fact]
    public async Task CalculateTaxAsyncReturnsHighEarnerTaxOver75()
    {
      var context = GetDbContext();

      context.TaxTableUploads.Add(new TaxTableUpload
      {
        TaxYear = 2026,
        FileName = "2026.xlsx",
        FileUrl = "2026.xlsx",
        EffectiveFrom = DateTime.UtcNow.AddDays(-1),
        EffectiveTo = null
      });

      context.TaxDeductions.Add(new TaxDeduction
      {
        TaxYear = 2026,
        Remuneration = 20000,
        AnnualEquivalent = 240000,
        TaxUnder65 = 2000,
        Tax65To74 = 1600,
        TaxOver75 = 1200
      });

      await context.SaveChangesAsync();

      var service = new TaxDeductionService(context);

      decimal highSalary = 500000;
      int age = 80;

      var tax = await service.CalculateTaxAsync(highSalary, age);

      decimal monthlyRem = highSalary / 12;
      decimal expectedTax = 53432 + 0.45m * (monthlyRem - 156328m / 12);
      expectedTax = Math.Floor(expectedTax);

      Assert.Equal(expectedTax, tax);
    }

  }
}

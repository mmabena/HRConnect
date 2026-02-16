namespace HRConnect.Api.Services
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Data;
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Repositories;
  using Microsoft.AspNetCore.Http;
  using Microsoft.EntityFrameworkCore;
  using OfficeOpenXml;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using System.ComponentModel;
  using System.Linq.Expressions;
  using System.Data.Common;
  using HRConnect.Api.Mappers;

  /// <summary>
  /// This service is responsible for handling tax deduction operations which includes:
  /// calculations of tax based on remuneration and age,
  /// retrieval, update of tax deduction data, and 
  /// upload/validation of Excel tax tables.
  /// </summary>
  public class TaxDeductionService : ITaxDeductionService
  {
    private readonly ITaxDeductionRepository _repository;

    /// <summary>
    /// Initializes a new instance of <see cref="TaxDeductionService"/> with the specified repository.
    /// </summary>
    /// <param name="repository">this is the repository instance for tax deductions</param>
    public TaxDeductionService(ITaxDeductionRepository repository)
    {
      _repository = repository;
    }

    /// <summary>
    /// Calculates the tax payable based on the 
    /// tax year(which is automatic based on the active tax table), remuneration, and age.
    /// Matches the remuneration to the correct tax bracket based on the upper bound.
    /// </summary>
    /// <param name="remuneration">Employee's salary</param>
    /// <param name="age">Employee's age</param>
    /// <returns>The tax amount applicable for the given parameter</returns>
    public async Task<decimal> CalculateTaxAsync(decimal remuneration, int age)
    {
      var today = DateTime.UtcNow.Date;

      // Find the active tax table for today
      var activeUploads = await _repository.GetActiveTaxTableUploadsAsync();
      var activeUpload = activeUploads
          .OrderByDescending(x => x.EffectiveFrom)
          .FirstOrDefault(x => x.EffectiveFrom <= today &&
                               (x.EffectiveTo == null || x.EffectiveTo >= today));

      if (activeUpload == null)
      {
        throw new ArgumentException("No active tax table found for the current date.");
      }

      int taxYear = activeUpload.TaxYear;

      // Try to find a tax row in the table
      var allDeductions = await _repository.GetTaxDeductionsByYearAsync(taxYear);
      var taxRow = allDeductions
                .Where(x => remuneration <= x.Remuneration)
                .OrderBy(x => x.Remuneration)
                .FirstOrDefault();

      if (taxRow != null)
      {
        //calculation
        return age switch
        {
          <= 64 => taxRow.TaxUnder65,
          <= 74 => taxRow.Tax65To74,
          _ => taxRow.TaxOver75
        };
      }
      else
      {
        // High-earner fallback calculation
        // [45% x (actual monthly remuneration - R156,328)] + age-specific base
        decimal monthlyRemuneration = remuneration / 12;

        decimal baseAmount = age switch
        {
          <= 64 => 54481m,
          <= 74 => 53694m,
          _ => 53432m
        };

        decimal excess = Math.Max(0, monthlyRemuneration - 156_328m / 12); // Monthly threshold
        decimal tax = baseAmount + (0.45m * excess);

        // Disregard cents (round down)
        return Math.Floor(tax);
      }
    }

    /// <summary>
    /// Retrieves all tax deductions for the tax year
    /// </summary>
    /// <param name="taxYear">The year to retrieve deductions for</param>
    /// <returns>List of tax deductions as DTOs</returns>
    public async Task<List<TaxDeductionDto>> GetAllTaxDeductionsAsync(int taxYear)
    {
      var entities = await _repository.GetTaxDeductionsByYearAsync(taxYear);
      var ordered = entities.OrderBy(x => x.Remuneration).ToList();
      return ordered.Select(TaxDeductionMapper.ToDto).ToList();
    }


    /// <summary>
    /// Updates a single tax deduction row with new values.
    /// </summary>
    /// <param name="dto">DTO containing updated tax deduction information.</param>
    /// <exception cref="ArgumentException">Thrown when the tax deduction row does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown if attempting to change the TaxYear.</exception>
    public async Task UpdateTaxDeductionAsync(UpdateTaxDeductionDto dto)
    {
      var deductions = await _repository.GetTaxDeductionsByYearAsync(dto.TaxYear);
      var entity = deductions.FirstOrDefault(x => x.Id == dto.Id);
      if (entity == null)
      {
        throw new ArgumentException("Tax deduction not found.");
      }

      if (entity.TaxYear != dto.TaxYear)
      {
        throw new InvalidOperationException("Cannot change TaxYear.");
      }

      entity.Remuneration = dto.Remuneration;
      entity.AnnualEquivalent = dto.AnnualEquivalent;
      entity.TaxUnder65 = dto.TaxUnder65;
      entity.Tax65To74 = dto.Tax65To74;
      entity.TaxOver75 = dto.TaxOver75;

      await _repository.SaveChangesAsync();
    }
  }
}

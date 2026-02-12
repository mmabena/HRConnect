namespace HRConnect.Api.Services
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Data;
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces;
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
    private readonly ITaxRepository _repository;

    /// <summary>
    /// Initializes a new instance of <see cref="TaxDeductionService"/> with the specified repository.
    /// </summary>
    /// <param name="repository">this is the repository instance for tax deductions</param>
    public TaxDeductionService(ITaxRepository repository)
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
      var activeUpload = await _repository.TaxTableUploads
          .Where(x =>
              x.EffectiveFrom <= today &&
              (x.EffectiveTo == null || x.EffectiveTo >= today))
          .OrderByDescending(x => x.EffectiveFrom)
          .FirstOrDefaultAsync();

      if (activeUpload == null)
      {
        throw new ArgumentException("No active tax table found for the current date.");
      }

      int taxYear = activeUpload.TaxYear;

      // Try to find a tax row in the table
      var taxRow = await _repository.TaxDeductions
          .Where(x => x.TaxYear == taxYear && remuneration <= x.Remuneration)
          .OrderBy(x => x.Remuneration)
          .FirstOrDefaultAsync();

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
      var entities = await _repository.TaxDeductions
      .Where(x => x.TaxYear == taxYear)
      .OrderBy(x => x.Remuneration)
      .ToListAsync();

      return entities
      .Select(TaxDeductionMapper.ToDto).ToList();
    }

    /// <summary>
    /// Updates a single tax deduction row with new values.
    /// </summary>
    /// <param name="dto">DTO containing updated tax deduction information.</param>
    /// <exception cref="ArgumentException">Thrown when the tax deduction row does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown if attempting to change the TaxYear.</exception>
    public async Task UpdateTaxDeductionAsync(UpdateTaxDeductionDto dto)
    {
      var entity = await _repository.TaxDeductions.FindAsync(dto.Id);
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

    /// <summary>
    /// Uploads a new tax table Excel file for a specific tax year.
    /// Validates headers and numeric data, and deactivates previous uploads.
    /// </summary>
    /// <param name="taxYear">The tax year for the upload.</param>
    /// <param name="file">The Excel file to upload.</param>
    /// <exception cref="ArgumentException">Thrown if the file type is invalid, headers are missing, or data is non-numeric.</exception>
    public async Task UploadTaxTableAsync(int taxYear, IFormFile file)
    {
      if (file == null)
        throw new ArgumentException("File is required.");

      var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
      if (extension != ".xlsx" && extension != ".xls")
        throw new ArgumentException("Only Excel files are allowed.");

      // Prevent duplicate tax year uploads
      var existingUploadForYear = await _repository.TaxTableUploads
          .AnyAsync(x => x.TaxYear == taxYear);

      if (existingUploadForYear)
      {
        throw new ArgumentException($"A tax table for year {taxYear} has already been uploaded.");
      }

      using var stream = new MemoryStream();
      await file.CopyToAsync(stream);

      using var package = new ExcelPackage(stream);
      var worksheet = package.Workbook.Worksheets.FirstOrDefault();

      if (worksheet == null)
      {
        throw new ArgumentException("Excel file contains no worksheets.");
      }

      var expectedHeaders = new[]
      {
        "Remuneration",
        "AnnualEquivalent",
        "TaxUnder65",
        "Tax65To74",
        "TaxOver75"
    };

      for (int i = 0; i < expectedHeaders.Length; i++)
      {
        if (worksheet.Cells[1, i + 1].Text != expectedHeaders[i])
          throw new ArgumentException(
              $"Invalid Excel format. Missing header: {expectedHeaders[i]}");
      }

      int rowCount = worksheet.Dimension.Rows;

      for (int row = 2; row <= rowCount; row++)
      {
        string remunerationText = worksheet.Cells[row, 1].Text.Trim();

        decimal remunerationUpper;
        try
        {
          var parts = remunerationText.Split('-');
          var upperPart = parts[1].Replace("R", "").Replace(",", "").Trim();
          remunerationUpper = decimal.Parse(
              upperPart,
              System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
          throw new ArgumentException(
              $"Invalid Remuneration range at row {row}: {remunerationText}");
        }

        decimal annualEquivalent = decimal.Parse(
            worksheet.Cells[row, 2].Text.Replace("R", "").Replace(",", "").Trim(),
            System.Globalization.CultureInfo.InvariantCulture);

        decimal taxUnder65 = decimal.Parse(
            worksheet.Cells[row, 3].Text.Replace("R", "").Replace(",", "").Trim(),
            System.Globalization.CultureInfo.InvariantCulture);

        decimal tax65To74 = decimal.Parse(
            worksheet.Cells[row, 4].Text.Replace("R", "").Replace(",", "").Trim(),
            System.Globalization.CultureInfo.InvariantCulture);

        decimal taxOver75 = decimal.Parse(
            worksheet.Cells[row, 5].Text.Replace("R", "").Replace(",", "").Trim(),
            System.Globalization.CultureInfo.InvariantCulture);

        _repository.TaxDeductions.Add(new TaxDeduction
        {
          TaxYear = taxYear,
          Remuneration = remunerationUpper,
          AnnualEquivalent = annualEquivalent,
          TaxUnder65 = taxUnder65,
          Tax65To74 = tax65To74,
          TaxOver75 = taxOver75,
          CreatedAt = DateTime.UtcNow
        });
      }

      // Financial-year logic
      var effectiveFrom = new DateTime(taxYear, 3, 1);
      var previousExpiry = new DateTime(taxYear, 2, 28);

      // Expire currently active table (if exists)
      var currentActive = await _repository.TaxTableUploads
          .Where(x => x.EffectiveTo == null)
          .OrderByDescending(x => x.EffectiveFrom)
          .FirstOrDefaultAsync();

      if (currentActive != null)
      {
        currentActive.EffectiveTo = previousExpiry;
      }

      var newUpload = new TaxTableUpload
      {
        TaxYear = taxYear,
        FileName = file.FileName,
        FileUrl = $"uploads/tax_{taxYear}.xlsx",
        EffectiveFrom = effectiveFrom,
        EffectiveTo = null,
        UploadedAt = DateTime.UtcNow
      };

      _repository.TaxTableUploads.Add(newUpload);

      await _repository.SaveChangesAsync();
    }
  }
}

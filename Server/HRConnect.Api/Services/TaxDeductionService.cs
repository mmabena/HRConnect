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
  using System.Runtime.CompilerServices;
  using System.ComponentModel;
  using System.Diagnostics.Contracts;
  using System.Linq.Expressions;
  using System.Data.Common;
  using HRConnect.Api.Mappers;
  using System.Security.Cryptography.X509Certificates;

  /// <summary>
  /// This service is responsible for handling tax deduction operations which includes:
  /// calculations of tax based on remuneration and age,
  /// retrieval, update of tax deduction data, and 
  /// upload/validation of Excel tax tables.
  /// </summary>
  public class TaxDeductionService : ITaxDeductionService
  {
    private readonly ApplicationDBContext _context;

    /// <summary>
    /// Constructor injecting the database context
    /// </summary>
    /// <param name="context">Application database context</param>
    public TaxDeductionService(ApplicationDBContext context)
    {
      _context = context;
    }

    /// <summary>
    /// Calculates the tax payable based on the tax year, remuneration, and age.
    /// Matches the remuneration to the correct tax bracket based on the upper bound.
    /// </summary>
    /// <param name="taxYear">The tax year being used</param>
    /// <param name="remuneration">Employee's salary</param>
    /// <param name="age">Employee's age</param>
    /// <returns>The tax amount applicable for the given parameter</returns>
    public async Task<decimal> CalculateTaxAsync(int taxYear, decimal remuneration, int age)
    {
      // Find the first tax bracket where the salary is less than or equal to the upper bound
      var taxRow = await _context.TaxDeductions
          .Where(x => x.TaxYear == taxYear && remuneration <= x.Remuneration)
          .OrderBy(x => x.Remuneration)
          .FirstOrDefaultAsync();

      if (taxRow == null)
        throw new ArgumentException("No tax bracket found for this remuneration.");

      // Return tax based on age
      return age switch
      {
        <= 64 => taxRow.TaxUnder65,
        <= 74 => taxRow.Tax65To74,
        _ => taxRow.TaxOver75
      };
    }


    /// <summary>
    /// Retrieves all tax deductions for the tax year
    /// </summary>
    /// <param name="taxYear">The year to retrieve deductions for</param>
    /// <returns>List of tax deductions as DTOs</returns>
    public async Task<List<TaxDeductionDto>> GetAllTaxDeductionsAsync(int taxYear)
    {
      var entities = await _context.TaxDeductions
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
      var entity = await _context.TaxDeductions.FindAsync(dto.Id);
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

      await _context.SaveChangesAsync();
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

      using var stream = new MemoryStream();
      await file.CopyToAsync(stream);

      using var package = new ExcelPackage(stream);
      var worksheet = package.Workbook.Worksheets.FirstOrDefault();

      if (worksheet == null)
        throw new ArgumentException("Excel file contains no worksheets.");

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
          remunerationUpper = decimal.Parse(upperPart, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
          throw new ArgumentException($"Invalid Remuneration range at row {row}: {remunerationText}");
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

        _context.TaxDeductions.Add(new TaxDeduction
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

      // Deactivate all active uploads, regardless of year
      var activeUploads = await _context.TaxTableUploads
          .Where(x => x.IsActive)
          .ToListAsync();

      activeUploads.ForEach(x => x.IsActive = false);

      var newUpload = new TaxTableUpload
      {
        TaxYear = taxYear,
        FileName = file.FileName,
        FileUrl = $"uploads/tax_{taxYear}.xlsx",
        IsActive = true,
        UploadedAt = DateTime.UtcNow
      };
      _context.TaxTableUploads.Add(newUpload);

      await _context.SaveChangesAsync();
    }
  }
}
namespace HRConnect.Api.Services
{
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Models;
  using HRConnect.Api.Repositories;
  using Microsoft.AspNetCore.Http;
  using Microsoft.EntityFrameworkCore;
  using OfficeOpenXml;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;

  /// <summary>
  /// This service is responsible for handling tax table upload operations which includes:
  /// retrieval of tax table uploads, and upload/validation of Excel tax tables.
  /// </summary>
  public class TaxTableUploadService : ITaxTableUploadService
  {
    private readonly ITaxTableUploadRepository _repository;
    private readonly ITaxDeductionRepository _deductionRepository;


    /// <summary>
    /// Initializes a new instance of <see cref="TaxTableUploadService"/> with the specified repositories.
    /// </summary>
    /// <param name="repository">The repository used to access tax table upload data.</param>
    /// <param name="deductionRepository">The repository used to access tax deduction data.</param>
    public TaxTableUploadService(ITaxTableUploadRepository repository, ITaxDeductionRepository deductionRepository)
    {
      _repository = repository;
      _deductionRepository = deductionRepository;
    }

    /// <summary>
    /// Retrieves all tax table uploads.
    /// </summary>
    /// <returns>
    /// A list of <see cref="TaxTableUploadDto"/> objects ordered by most recent upload.
    /// </returns>
    public async Task<List<TaxTableUploadDto>> GetAllUploadsAsync()
    {
      var uploads = await _repository.GetAllAsync();
      return uploads.Select(TaxTableUploadMapper.ToDto).ToList();
    }

    /// <summary>
    /// Retrieves the active tax table upload for a given tax year.
    /// </summary>
    /// <param name="taxYear">The tax year to retrieve.</param>
    /// <returns>
    /// A <see cref="TaxTableUploadDto"/> if an active upload exists; otherwise <c>null</c>.
    /// </returns>
    public async Task<TaxTableUploadDto?> GetUploadByYearAsync(int taxYear)
    {
      var upload = await _repository.GetActiveByYearAsync(taxYear);
      return upload == null ? null : TaxTableUploadMapper.ToDto(upload);
    }

    /// <summary>
    /// Uploads a new tax table Excel file for a specific tax year.
    /// Validates headers and numeric data, and deactivates previous uploads.
    /// </summary>
    /// <param name="taxYear">The tax year for the upload.</param>
    /// <param name="file">The Excel file to upload.</param>
    /// <exception cref="ArgumentException">Thrown if the file type is invalid, headers are missing, or data is non-numeric.</exception>
    public async Task<TaxTableUploadResultDto> UploadTaxTableAsync(int taxYear, IFormFile file)
    {
      if (file == null)
      {
        throw new ArgumentException("File is required.");
      }

      var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
      if (extension != ".xlsx" && extension != ".xls")
      {
        throw new ArgumentException("Only Excel files are allowed.");
      }

      // Prevent duplicate tax year uploads
      var existingUploadForYear = (await _repository.GetActiveTaxTableUploadsAsync())
          .Any(x => x.TaxYear == taxYear);
      if (existingUploadForYear)
      {
        throw new ArgumentException($"A tax table for year {taxYear} has already been uploaded.");
      }

      //tax year can not be less than 2000 or more than the current year + 1(if we in 2026 you 
      // can not upload for 2028)
      if (taxYear < 2000 || taxYear > DateTime.UtcNow.Year + 1)
      {
        throw new ArgumentException("Invalid tax year.");
      }

      using var stream = new MemoryStream();
      await file.CopyToAsync(stream);

      //read it as an excel file
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
        {
          throw new ArgumentException($"Invalid Excel format. Missing header: {expectedHeaders[i]}");
        }
      }

      int rowCount = worksheet.Dimension.Rows;
      var deductions = new List<TaxDeduction>();

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

        deductions.Add(new TaxDeduction
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

      // Add all deductions at once
      await _deductionRepository.AddTaxDeductionsAsync(deductions);

      // Financial-year logic
      var effectiveFrom = new DateTime(taxYear, 3, 1);
      var previousExpiry = new DateTime(taxYear, 2, 28);

      // Expire currently active table (if exists)
      var activeUploads = await _repository.GetActiveTaxTableUploadsAsync();
      var currentActive = activeUploads
          .Where(x => x.EffectiveTo == null)
          .OrderByDescending(x => x.EffectiveFrom)
          .FirstOrDefault();

      if (currentActive != null)
        currentActive.EffectiveTo = previousExpiry;

      var newUpload = new TaxTableUpload
      {
        TaxYear = taxYear,
        FileName = file.FileName,
        FileUrl = $"uploads/tax_{taxYear}.xlsx",
        EffectiveFrom = effectiveFrom,
        EffectiveTo = null,
        UploadedAt = DateTime.UtcNow
      };

      await _repository.AddTaxTableUploadAsync(newUpload);
      await _repository.SaveChangesAsync();

      return new TaxTableUploadResultDto
      {
        Message = $"Tax table for the year {taxYear} uploaded successfully.",
        EffectiveFrom = effectiveFrom
      };
    }
  }
}
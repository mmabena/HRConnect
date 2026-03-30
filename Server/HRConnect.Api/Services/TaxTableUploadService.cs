namespace HRConnect.Api.Services
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Models;
  using HRConnect.Api.Repositories;
  using Microsoft.AspNetCore.Http;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;
  using OfficeOpenXml;
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;

  /// <summary>
  /// Handles tax table upload operations: retrieval, validation, and storage of Excel tax tables.
  /// </summary>
  public class TaxTableUploadService : ITaxTableUploadService
  {
    private readonly ITaxTableUploadRepository _repository;
    private readonly ITaxDeductionRepository _deductionRepository;
    private readonly ILogger<TaxTableUploadService> _logger;

    public TaxTableUploadService(
        ITaxTableUploadRepository repository,
        ITaxDeductionRepository deductionRepository,
        ILogger<TaxTableUploadService> logger)
    {
      _repository = repository;
      _deductionRepository = deductionRepository;
      _logger = logger;
    }

    public async Task<List<TaxTableUploadDto>> GetAllUploadsAsync()
    {
      var uploads = await _repository.GetAllAsync();
      return uploads.Select(TaxTableUploadMapper.ToDto).ToList();
    }

    public async Task<TaxTableUploadDto?> GetUploadByYearAsync(int taxYear)
    {
      var upload = await _repository.GetActiveByYearAsync(taxYear);
      return upload == null ? null : TaxTableUploadMapper.ToDto(upload);
    }

    public async Task<TaxTableUploadResultDto> UploadTaxTableAsync(int taxYear, IFormFile file)
    {
      if (file == null)
        throw new ArgumentException("File is required.");

      var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
      if (extension != ".xlsx" && extension != ".xls")
        throw new ArgumentException("Only Excel files are allowed.");

      if (taxYear < 2000 || taxYear > DateTime.UtcNow.Year + 1)
        throw new ArgumentException("Invalid tax year.");

      var existingUploadForYear = (await _repository.GetActiveTaxTableUploadsAsync())
          .Any(x => x.TaxYear == taxYear);
      if (existingUploadForYear)
        throw new ArgumentException($"A tax table for year {taxYear} has already been uploaded.");

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
        var cellValue = worksheet.Cells[1, i + 1].Text.Trim();
        if (!string.Equals(cellValue, expectedHeaders[i], StringComparison.OrdinalIgnoreCase))
          throw new ArgumentException($"Invalid Excel format. Missing or incorrect header: {expectedHeaders[i]}");
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
          if (parts.Length < 2)
            throw new FormatException();

          var upperPart = parts[1].Replace("R", "").Replace(",", "").Trim();
          remunerationUpper = ParseCurrency(upperPart);
        }
        catch
        {
          throw new ArgumentException($"Invalid Remuneration range at row {row}: {remunerationText}");
        }

        var annualEquivalent = ParseCurrency(worksheet.Cells[row, 2].Text);
        var taxUnder65 = ParseCurrency(worksheet.Cells[row, 3].Text);
        var tax65To74 = ParseCurrency(worksheet.Cells[row, 4].Text);
        var taxOver75 = ParseCurrency(worksheet.Cells[row, 5].Text);

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

      var effectiveFrom = new DateTime(taxYear, 3, 1, 0, 0, 0, DateTimeKind.Utc);
      var today = DateTime.UtcNow.Date;

      using var transaction = await _repository.BeginTransactionAsync();
      try
      {
        await _deductionRepository.AddTaxDeductionsAsync(deductions);

        if (effectiveFrom <= today)
        {
          var activeUploads = await _repository.GetActiveTaxTableUploadsAsync();
          var currentActive = activeUploads
              .Where(x => x.EffectiveTo == null)
              .OrderByDescending(x => x.EffectiveFrom)
              .FirstOrDefault();

          if (currentActive != null)
            currentActive.EffectiveTo = effectiveFrom.AddDays(-1);
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

        await _repository.AddTaxTableUploadAsync(newUpload);
        await _repository.SaveChangesAsync();

        await transaction.CommitAsync();

        _logger.LogInformation("Tax table for year {TaxYear} uploaded successfully.", taxYear);

        return new TaxTableUploadResultDto
        {
          Message = $"Tax table for the year {taxYear} uploaded successfully.",
          EffectiveFrom = effectiveFrom
        };
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Failed to upload tax table for year {TaxYear}.", taxYear);
        throw;
      }
    }

    private static decimal ParseCurrency(string input)
    {
      var cleaned = input.Replace("R", "").Replace(",", "").Trim();
      return decimal.Parse(cleaned, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
    }
  }
}

namespace HRConnect.Api.Services
{
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Models;
  using Microsoft.AspNetCore.Http;
  using Microsoft.EntityFrameworkCore;
  using OfficeOpenXml;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;


  public class TaxTableUploadService : ITaxTableUploadService
  {
    private readonly ApplicationDBContext _context;

    /// <summary>
    /// Initializes a new instance of the TaxTableUploadService
    /// </summary>
    /// <param name="context">The application database context</param>
    public TaxTableUploadService(ApplicationDBContext context)
    {
      _context = context;
    }

    /// <summary>
    /// Retrievers all tax table uploads
    /// </summary>
    /// <returns>A list of the tax table upload DTOs</returns>
    public async Task<List<TaxTableUploadDto>> GetAllUploadAsync()
    {
      var uploads = await _context.TaxTableUploads
      .OrderByDescending(x => x.UploadedAt).ToListAsync();

      return uploads
      .Select(TaxTableUploadMapper.ToDto).ToList();
    }

    /// <summary>
    /// Retrieves the active tax table upload for a specific tax year.
    /// </summary>
    /// <param name="taxYear">The tax year to retreive </param>
    /// <returns>The tax table upload DTO if found; otherwise its null.</returns>
    public async Task<TaxTableUploadDto?> GetUploadByYearAsync(int taxYear)
    {
      var upload = await _context.TaxTableUploads
      .FirstOrDefaultAsync(x => x.TaxYear == taxYear && x.IsActive);

      return upload == null ? null : TaxTableUploadMapper.ToDto(upload);
    }

    /// <summary>
    /// Uploads a new Excel tax table for a given tax year
    /// Ensures only one active upload exists per year
    /// </summary>
    /// <param name="taxYear">The tax year for the upload</param>
    /// <param name="file">The excel file containing the tax table</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">thrown when the file is invalid or does not 
    /// match the expected format</exception>
    public async Task UploadTaxTableAsync(int taxYear, IFormFile file)
    {
      var existsForYear = await _context
      .TaxTableUploads.AnyAsync(x => x.TaxYear == taxYear && x.IsActive);

      if (existsForYear)
      {
        throw new InvalidOperationException($"A tax table for the year {taxYear} already exists");
      }
      
      if (file == null)
      {
        throw new ArgumentException("File is required.");
      }

      var extension = Path
      .GetExtension(file.FileName)
      .ToLowerInvariant();

      if (extension != ".xlsx" && extension != ".xls")
      {
        throw new ArgumentException("Only Excel files are allowed.");
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
        {
          throw new ArgumentException(
              $"Invalid Excel format. Missing header: {expectedHeaders[i]}");
        }
      }

      // Deactivate existing uploads for the year
      var existingUploads = await _context.TaxTableUploads
          .Where(x => x.TaxYear == taxYear && x.IsActive)
          .ToListAsync();

      existingUploads.ForEach(x => x.IsActive = false);

      var upload = new TaxTableUpload
      {
        TaxYear = taxYear,
        FileName = file.FileName,
        FileUrl = $"uploads/tax_{taxYear}.xlsx",
        IsActive = true,
        UploadedAt = DateTime.UtcNow
      };

      _context.TaxTableUploads.Add(upload);
      await _context.SaveChangesAsync();
    }

    public async Task<List<TaxTableUploadDto>> GetAllUploadsAsync()
    {
      var uploads = await _context.TaxTableUploads
          .OrderByDescending(x => x.TaxYear)
          .ToListAsync();

      return uploads.Select(x => new TaxTableUploadDto
      {
        Id = x.Id,
        TaxYear = x.TaxYear,
        FileName = x.FileName,
        FileUrl = x.FileUrl
      }).ToList();
    }

  }
}
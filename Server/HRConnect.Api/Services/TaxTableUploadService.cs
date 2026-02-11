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
    public async Task<List<TaxTableUploadDto>> GetAllUploadsAsync()
    {
      var uploads = await _context.TaxTableUploads
      .OrderByDescending(x => x.UploadedAt).ToListAsync();

      return uploads
      .Select(TaxTableUploadMapper.ToDto).ToList();
    }

    /// <summary>
    /// Retrieves the active tax table that is effective for that tax year..
    /// </summary>
    /// <param name="taxYear">The tax year to retreive </param>
    /// <returns>The tax table upload DTO if found; otherwise its null.</returns>
    public async Task<TaxTableUploadDto?> GetUploadByYearAsync(int taxYear)
    {
      var today = DateTime.UtcNow.Date;

      var upload = await _context.TaxTableUploads
          .Where(x => x.TaxYear == taxYear &&
                      x.EffectiveFrom <= today &&
                      (x.EffectiveTo == null || x.EffectiveTo >= today))
          .OrderByDescending(x => x.EffectiveFrom)
          .FirstOrDefaultAsync();

      return upload == null ? null : TaxTableUploadMapper.ToDto(upload);
    }
  }
}
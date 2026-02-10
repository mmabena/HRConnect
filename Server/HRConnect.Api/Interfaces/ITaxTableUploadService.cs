namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Http;


  /// <summary>
  /// Defines operations for managing uploaded tax table files.
  /// This includes retrieving tax table uploads and handling.
  /// controlled yearly uploads of Excel-based tyax tables
  /// </summary>
  public interface ITaxTableUploadService
  {
    Task<List<TaxTableUploadDto>> GetAllUploadAsync();
    Task<TaxTableUploadDto?> GetUploadByYearAsync(int taxYear);
    Task UploadTaxTableAsync(int taxYear, IFormFile file);
    Task<List<TaxTableUploadDto>> GetAllUploadsAsync();
  }
}
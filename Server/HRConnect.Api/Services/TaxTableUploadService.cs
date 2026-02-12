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
    private readonly ITaxTableUploadRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaxTableUploadService"/>.
    /// </summary>
    /// <param name="repository">
    /// The repository used to access tax table upload data.
    /// </param>
    public TaxTableUploadService(ITaxTableUploadRepository repository)
    {
      _repository = repository;
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
  }
}
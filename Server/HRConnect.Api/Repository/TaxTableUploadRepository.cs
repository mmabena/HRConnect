using HRConnect.Api.Data;
using HRConnect.Api.Interfaces;
using HRConnect.Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRConnect.Api.Repositories
{
  /// <summary>
  /// Repository implementation for managing <see cref="TaxTableUpload"/> entities.
  /// </summary>
  public class TaxTableUploadRepository : ITaxTableUploadRepository
  {
    private readonly ApplicationDBContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaxTableUploadRepository"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public TaxTableUploadRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task<List<TaxTableUpload>> GetAllAsync()
    {
      return await _context.TaxTableUploads
          .OrderByDescending(x => x.UploadedAt)
          .ToListAsync();
    }

    public async Task<TaxTableUpload?> GetActiveByYearAsync(int taxYear)
    {
      var today = DateTime.UtcNow.Date;

      return await _context.TaxTableUploads
          .Where(x => x.TaxYear == taxYear &&
                      x.EffectiveFrom <= today &&
                      (x.EffectiveTo == null || x.EffectiveTo >= today))
          .OrderByDescending(x => x.EffectiveFrom)
          .FirstOrDefaultAsync();
    }
  }
}

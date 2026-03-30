namespace HRConnect.Api.Repositories
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  /// <summary>
  /// Repository implementation for EF Core access to TaxDeductions and related TaxTableUploads.
  /// </summary>
  public class TaxDeductionRepository : ITaxDeductionRepository
  {
    private readonly ApplicationDBContext _context;

    /// <summary>
    /// Initializes a new instance of the TaxTableUploadRepository class with the specified database context.
    /// </summary>
    /// <param name="context">The database context to use for repository operations.</param>
    public TaxDeductionRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task<List<TaxDeduction>> GetTaxDeductionsByYearAsync(int taxYear)
    {
      return await _context.TaxDeductions
          .Where(x => x.TaxYear == taxYear)
          .OrderBy(x => x.Remuneration)
          .ToListAsync();
    }

    public async Task<List<TaxTableUpload>> GetActiveTaxTableUploadsAsync()
    {
      var today = DateTime.UtcNow.Date;
      return await _context.TaxTableUploads
          .Where(x => x.EffectiveFrom <= today &&
                      (x.EffectiveTo == null || x.EffectiveTo >= today))
          .OrderByDescending(x => x.EffectiveFrom)
          .ToListAsync();
    }

    public Task DeactivateTaxTableUploadsAsync(List<TaxTableUpload> uploads)
    {
      var expiryDate = DateTime.UtcNow.Date.AddDays(-1);
      uploads.ForEach(x => x.EffectiveTo = expiryDate);
      return Task.CompletedTask;
    }

    public async Task AddTaxTableUploadAsync(TaxTableUpload upload)
    {
      await _context.TaxTableUploads.AddAsync(upload);
    }

    public async Task AddTaxDeductionsAsync(List<TaxDeduction> deductions)
    {
      await _context.TaxDeductions.AddRangeAsync(deductions);
    }

    public async Task SaveChangesAsync()
    {
      await _context.SaveChangesAsync();
    }
  }
}

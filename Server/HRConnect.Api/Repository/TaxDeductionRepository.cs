using HRConnect.Api.Data;
using HRConnect.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRConnect.Api.Repositories
{
  /// <summary>
  /// implementation of <see cref="ITaxRepository"/> for EF Core database access.
  /// </summary>
  public class TaxRepository : ITaxRepository
  {
    private readonly ApplicationDBContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="TaxRepository"/>.
    /// </summary>
    /// <param name="context">The application's database context.</param>
    public TaxRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task<List<TaxTableUpload>> GetActiveTaxTableUploadsAsync()
    {
      return await _context.TaxTableUploads
          .Where(x => x.IsActive)
          .OrderByDescending(x => x.UploadedAt)
          .ToListAsync();
    }

    public async Task<List<TaxDeduction>> GetTaxDeductionsByYearAsync(int taxYear)
    {
      return await _context.TaxDeductions
          .Where(x => x.TaxYear == taxYear)
          .OrderBy(x => x.Remuneration)
          .ToListAsync();
    }

    public async Task AddTaxTableUploadAsync(TaxTableUpload upload)
    {
      await _context.TaxTableUploads.AddAsync(upload);
    }

    public async Task AddTaxDeductionsAsync(List<TaxDeduction> deductions)
    {
      await _context.TaxDeductions.AddRangeAsync(deductions);
    }

    public async Task DeactivateTaxTableUploadsAsync(List<TaxTableUpload> uploads)
    {
      uploads.ForEach(x => x.IsActive = false);
    }

    public async Task SaveChangesAsync()
    {
      await _context.SaveChangesAsync();
    }
  }
}

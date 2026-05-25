namespace HRConnect.Api.Repositories
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Models;

  /// <summary>
  /// Provides data access operations for <see cref="TaxTableUpload"/> entities.
  /// </summary>
  public interface ITaxTableUploadRepository
  {
    Task<List<TaxTableUpload>> GetActiveTaxTableUploadsAsync();
    Task<List<TaxTableUpload>> GetAllAsync();
    Task<TaxTableUpload?> GetActiveByYearAsync(int taxYear);
    Task AddTaxTableUploadAsync(TaxTableUpload upload); 
    Task DeactivateTaxTableUploadsAsync(List<TaxTableUpload> uploads);
    Task SaveChangesAsync();
  }
}

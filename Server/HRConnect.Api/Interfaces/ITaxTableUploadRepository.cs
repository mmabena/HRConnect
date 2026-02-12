namespace HRConnect.Api.Repositories
{
  using HRConnect.Api.Models;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  
  /// <summary>
  /// Provides data access operations for <see cref="TaxTableUpload"/> entities.
  /// </summary>
  public interface ITaxTableUploadRepository
  {
    Task<List<TaxTableUpload>> GetAllAsync();
    Task<TaxTableUpload?> GetActiveByYearAsync(int taxYear);
  }
}

namespace HRConnect.Api.Repositories
{
  using HRConnect.Api.Models;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  /// <summary>
  /// Repository interface for accessing TaxTableUploads and TaxDeductions in the database.
  /// </summary>
  public interface ITaxDeductionRepository
  {
    Task<List<TaxTableUpload>> GetActiveTaxTableUploadsAsync(); 
    Task<List<TaxDeduction>> GetTaxDeductionsByYearAsync(int taxYear); 
    Task AddTaxTableUploadAsync(TaxTableUpload upload);
    Task AddTaxDeductionsAsync(List<TaxDeduction> deductions);
    Task DeactivateTaxTableUploadsAsync(List<TaxTableUpload> uploads);
    Task SaveChangesAsync();
  }
}

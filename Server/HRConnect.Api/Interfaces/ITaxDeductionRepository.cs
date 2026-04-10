namespace HRConnect.Api.Repositories
{
  using HRConnect.Api.Models;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Models.PayrollDeduction;

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

    Task<Employee?> GetEmployeeByIdAsync(int employeeId);
    Task<PensionDeduction?> GetPensionByEmployeeIdAsync(int employeeId);
    Task AddFinalTaxDeductionAsync(FinalTaxDeduction deduction);
    Task<FinalTaxDeduction?> GetExistingFinalTaxAsync(int employeeId, int payRunId);
  }
}

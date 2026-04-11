namespace HRConnect.Api.Repositories
{
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
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

    Task<Employee?> GetEmployeeByEmailAsync(string email);
    Task<PayrollRun?> GetActivePayrollRunAsync();
    Task<PensionDeduction?> GetPensionByEmployeeIdAsync(string employeeId);
    Task AddFinalTaxDeductionAsync(FinalTaxDeduction deduction);
    Task<FinalTaxDeduction?> GetExistingFinalTaxAsync(string employeeId, int payRunId);
  }
}

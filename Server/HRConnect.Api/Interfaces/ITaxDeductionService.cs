namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.TaxDeduction;
  using HRConnect.Api.Models.PayrollDeduction;

  /// <summary>
  /// Defines operations related to tax deductions, including
  /// tax calculation, retrieval, updates, and tax table uploads.
  /// </summary>
  public interface ITaxDeductionService
  {
    Task<decimal> CalculateTaxAsync(decimal remuneration, int age);
    Task<List<TaxDeductionDto>> GetAllTaxDeductionsAsync(int taxYear);
    Task UpdateTaxDeductionAsync(UpdateTaxDeductionDto dto);
    Task<FinalTaxDeduction> GenerateTaxAsync(TaxCalculationDto  request);
  }
}
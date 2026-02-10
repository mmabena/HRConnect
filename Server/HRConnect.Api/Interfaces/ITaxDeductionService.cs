namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Models;

  /// <summary>
  /// Defines operations related to tax deductions, including
  /// tax calculation, retrieval, updates, and tax table uploads.
  /// </summary>
  public interface ITaxDeductionService
  {
    Task<decimal> CalculateTaxAsync(int taxYear, decimal remuneration, int age);
    Task<List<TaxDeductionDto>> GetAllTaxDeductionsAsync(int taxYear);
    Task UpdateTaxDeductionAsync(UpdateTaxDeductionDto dto);
    Task UploadTaxTableAsync(int taxYear, IFormFile file);
  }
}
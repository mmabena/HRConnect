namespace HRConnect.Api.Interfaces.Payroll.Deduction
{
  using HRConnect.Api.DTOs.Payroll.Deduction;

  public interface IDeductionService
  {
    Task<DeductionDto> AddAsync(DeductionAddDto deductionAddDto);
    Task<List<DeductionDto>> GetAllDeductionsAsync();
    Task<List<DeductionDto>> GetDeductionsByCompanyIdAsync(string companyId);
    Task<DeductionDto?> GetDeductionByCodeAsync(string code);
    Task<DeductionDto> UpdateAsync(DeductionUpdateDto deductionUpdateDto);
    Task<string> DeleteAsync(string code);
  }
}

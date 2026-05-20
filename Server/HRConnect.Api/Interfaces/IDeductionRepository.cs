namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models.PayrollDeduction;

  public interface IDeductionRepository
  {
    Task<Deduction> AddAsync(Deduction deduction);
    Task<bool> CheckIfDescriptionsExists(string shortDescription, string longDescription);
    Task<List<Deduction>> GetAllDeductionsAsync();
    Task<Deduction?> GetDeductionByCodeAsync(string code);
    Task<List<Deduction>> GetDeductionByCompanyIdAsync(string companyId);
    Task<List<string>> GetAllDeductionCodesAsync(string prefix);
    Task<Deduction> UpdateAsync(Deduction deduction);
    Task<string> DeleteAsync(string code);
  }
}

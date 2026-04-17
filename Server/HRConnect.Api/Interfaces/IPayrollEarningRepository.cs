namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models.Payroll.Earning;

  public interface IPayrollEarningRepository
  {
    Task<PayrollEarning> AddAsync(PayrollEarning payrollEarning);
    Task<PayrollEarning?> GetByPayrollEarningId(string payrollEarningId);
    Task<List<PayrollEarning>> GetByTaxCode(int taxCode);
    Task<List<PayrollEarning>> GetAllAsync();
    Task<List<string>> GetAllPayrollEarningIdsAsync(string prefix);
    Task<PayrollEarning> UpdateAsync(PayrollEarning payrollEarning);
    Task<string> DeleteAsync(string payrollEarningId);
    Task<bool> CheckIfDescriptionsExists(string shortDescription, string longDescription);
  }
}

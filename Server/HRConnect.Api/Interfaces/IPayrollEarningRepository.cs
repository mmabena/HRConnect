namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models.Payroll.Earnings;

  public interface IPayrollEarningRepository
  {
    Task<PayrollEarning> AddAsync(PayrollEarning payrollEarning);
    Task<PayrollEarning?> GetByPayrollEarningId(string payrollEarningId);
    Task<List<PayrollEarning>> GetByTaxCode(int taxCode);
    Task<List<PayrollEarning>> GetAllAsync();
    Task<PayrollEarning> UpdateAsync(PayrollEarning payrollEarning);
  }
}

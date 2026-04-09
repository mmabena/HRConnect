namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models.Payroll;

  public interface IPayrollRunRepository
  {
    Task<PayrollRun?> GetPayrunByRunNumberAsync(int payrollRunNumber);

    Task<PayrollRun?> GetUnlockedPayrunByRunNumberAsync(int payrollRunNumber);
    Task<IEnumerable<PayrollRun>> GetAllPayruns();
    Task<PayrollRun> CreatePayrollRunAsync(PayrollRun payrollRun);
    Task<PayrollRun?> GetRunByDateAsync(int payrollRunNumber, DateTime startDate, DateTime endDate);
    Task<PayrollRun?> GetCurrentRunAsync();
    Task<PayrollRun?> GetLastPayrun();
    Task<PayrollRun?> IsExpiredPayRunUnlocked();
    Task UpdateRun(PayrollRun payrollRun);
    Task UpdateExpiredRun(PayrollRun payrollRun);
  }
}
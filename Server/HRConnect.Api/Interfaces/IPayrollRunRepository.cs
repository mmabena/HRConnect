namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models.Payroll;

  public interface IPayrollRunRepository
  {
    Task<PayrollRun?> GetPayrunByRunNumberAsync(int id);
    Task<IEnumerable<PayrollRun>> GetAllPayruns();
    /// CONSIDER CHANGING THE RETURN TYPE OF THIS TASK
    Task<PayrollRun> CreatePayrollRunAsync(PayrollRun payrollRun);
    Task<PayrollRun?> GetRunByDateAsync(DateTime dateTime);
    Task<PayrollRun?> GetCurrentRunAsync();
    Task UpdateRun(PayrollRun payrollRun);
    // Task AddRecordToCurrentRunAsync(PayrollRecord payrollRecord);
    Task<PayrollRun?> GetLastPayrun();
    Task<PayrollRun> GetAllPayRecordsFromPayRun(PayrollRun payrollRun);
    Task<PayrollRun?> IsExpiredPayRunUnlocked();
    Task UpdateExpiredRun(PayrollRun payrollRun);
  }
}
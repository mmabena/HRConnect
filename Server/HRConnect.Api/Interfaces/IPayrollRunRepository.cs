namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models.Payroll;

  public interface IPayrollRunRepository
  {
    Task<PayrollRun?> GetPayrunByIdAsync(int id);
    Task<IEnumerable<PayrollRun>> GetAllPayruns();
    /// CONSIDER CHANGING THE RETURN TYPE OF THIS TASK
    Task<PayrollRun> CreatePayrollRunAsync(PayrollRun payrollRun);
    Task<PayrollRun?> GetRunByDateAsync(DateTime dateTime);
    Task<PayrollRun?> GetCurrentRunAsync();
    Task UpdateRunAsync(PayrollRun payrollRun);
    Task AddRecordToCurrentRunAsync(PayrollRecord payrollRecord);
    Task<PayrollRun?> GetLastPayrun();
  }
}
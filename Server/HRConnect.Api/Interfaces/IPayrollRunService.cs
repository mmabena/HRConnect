namespace HRConnect.Api.Interfaces
{
  using DTOs.Payroll;
  using Models.Payroll;

  public interface IPayrollRunService
  {
    Task<PayrollRunDto?> GetPayrunByIdAsync(int id);
    Task<IEnumerable<PayrollRunDto>> GetAllPayruns();
    /// CONSIDER CHANGING THE RETURN TYPE OF THIS TASK
    Task<PayrollRunDto> CreatePayrollRunAsync(PayrollRun payrollRun);
    Task<PayrollRun?> GetRunByDateAsync(DateTime dateTime);
    Task<PayrollRun> GetCurrentRunAsync();
    Task UpdateRunAsync(PayrollRun payrollRun);
    Task AddRecordToCurrentRunAsync(PayrollRecord payrollRecord);
    Task<PayrollRun> GetAllPayRecordsFromPayRunAsync(int payrollRunId);
  }
}
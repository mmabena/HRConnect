namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.DTOs.Payroll;

  public interface IPayrollRunService
  {
    Task<PayrollRunDto?> GetPayrunByRunNumberAsync(int id);
    Task<IEnumerable<PayrollRunDto>> GetAllPayruns();
    /// CONSIDER CHANGING THE RETURN TYPE OF THIS TASK
    Task<PayrollRun> CreatePayrollRunAsync(PayrollRun payrollRun);
    Task<PayrollRunDto?> RequestRunByDateAsync(PayrollRunRequestDto dto);
    Task<PayrollRun> GetCurrentRunAsync();
    Task UpdateRunAsync(PayrollRun payrollRun);
    Task AddRecordToCurrentRunAsync(PayrollRecord payrollRecord, string employeeId);
    [Obsolete("All PayrollRecord's should be queried using their respective Repository or Service calls")]
    Task<PayrollRun> GetAllPayRecordsFromPayRunAsync(int payrollRunNumber);
    Task LockAllOlderPayrollRuns();
  }
}
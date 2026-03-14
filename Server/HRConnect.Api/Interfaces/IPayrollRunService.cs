namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.DTOs.Payroll;

  public interface IPayrollRunService
  {
    Task<PayrollRunDto?> GetPayrunByIdAsync(int id);
    Task<IEnumerable<PayrollRunDto>> GetAllPayruns();
    /// CONSIDER CHANGING THE RETURN TYPE OF THIS TASK
    Task<PayrollRunDto> CreatePayrollRunAsync(PayrollRun payrollRun);
    Task<PayrollRun?> GetRunByDateAsync(DateTime dateTime);
    Task<PayrollRun> GetCurrentRunAsync();
    Task UpdateRunAsync(PayrollRun payrollRun);
    Task AddRecordToCurrentRunAsync(PayrollRecord payrollRecord, string employeeId);
    Task<PayrollRun> GetAllPayRecordsFromPayRunAsync(int payrollRunNumber);
  }
}
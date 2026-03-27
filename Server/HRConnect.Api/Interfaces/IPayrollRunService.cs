namespace HRConnect.Api.Interfaces
{
  using DTOs.Payroll;
  using Models.Payroll;

  public interface IPayrollRunService
  {
    Task<PayrollRunDto?> GetPayrunByRunNumberAsync(int id);
    Task<IEnumerable<PayrollRunDto>> GetAllPayruns();
    /// CONSIDER CHANGING THE RETURN TYPE OF THIS TASK
    Task<PayrollRun> CreatePayrollRunAsync(PayrollRun payrollRun);
    Task<PayrollRunDto?> GetRunByDateAsync(DateTime dateTime);
    Task<PayrollRun> GetCurrentRunAsync();
    Task UpdateRunAsync(PayrollRun payrollRun);
    Task AddRecordToCurrentRunAsync(PayrollRecord payrollRecord, string employeeId);

    [Obsolete(
      "All PayrollRecord's should be queried using their respective Repository or Service calls")]
    Task AddRecordsCollectionToRunAsync(IList<PayrollRecord> recordsCollection);
    Task<PayrollRun> GetAllPayRecordsFromPayRunAsync(int payrollRunNumber);
    Task LockAllOlderPayrollRuns();
  }
}
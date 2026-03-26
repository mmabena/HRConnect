namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.DTOs.Payroll;

  /// <summary>
  /// Payroll Service functionality needed for adding payroll records to the payroll.
  /// Records added by this service are locked, archived and rollover every 1st day
  ///  of the month at 00:00
  /// </summary>
  /// <remarks>
  /// This service DOES NOT contain any logic to copy the previous month's payroll
  /// records into the current month's payroll records collection
  /// </remarks>
  public interface IPayrollRunService
  {
    Task<PayrollRunDto?> GetPayrunByRunNumberAsync(int id);
    Task<IEnumerable<PayrollRunDto>> GetAllPayruns();
    // Task<PayrollRun> CreatePayrollRunAsync(PayrollRun payrollRun);

    /// <summary>
    /// This service method adds a payroll record type to the current active payroll run's 'Records' collections.
    ///  All records added the collection are automatically locked. 
    /// </summary>
    /// <param name="payrollRecord">Payroll Record derived type being added 
    /// to the current payroll run  </param>
    /// <param name="employeeId">EmployeeId as a foreign for adding record for particular record</param>
    /// <returns>Successfully Completed Task</returns>
    /// <exception cref="InvalidDataException">Invalid Type Expected 'PayrollRecord'
    /// </exception>
    Task<PayrollRunDto?> RequestRunByDateAsync(PayrollRunRequestDto dto);
    Task<PayrollRun> GetCurrentRunAsync();
    Task UpdateRunAsync(PayrollRun payrollRun);

    /// <summary>
    /// This service method adds a payroll record type to the current active payroll run's 'Records' collections.
    ///  All records added the collection are automatically locked. 
    /// </summary>
    /// <param name="payrollRecord">Payroll Record derived type being added 
    /// to the current payroll run  </param>
    /// <param name="employeeId">EmployeeId as a foreign for adding record for particular record</param>
    /// <returns>Successfully Completed Task</returns>
    /// <exception cref="InvalidDataException">Invalid Type Expected 'PayrollRecord'
    /// </exception>
    Task AddRecordToCurrentRunAsync(PayrollRecord payrollRecord, string employeeId);
    [Obsolete("All PayrollRecord's should be queried using their respective Repository or Service calls")]
    Task<PayrollRun> GetAllPayRecordsFromPayRunAsync(int payrollRunNumber);
    Task LockAllOlderPayrollRuns();
  }
}
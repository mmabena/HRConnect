namespace HRConnect.Api.Interfaces.Payroll.Earnings
{
  using HRConnect.Api.DTOs.Payroll.Earnings;

  public interface IEmployeePayrollEarningService
  {
    Task<EmployeePayrollEarningDto> AddAsync(EmployeePayrollEarningAddDto employeePayrollEarningAddDto);
    Task<List<EmployeePayrollEarningDto>> GetAllAsync();
    Task<List<EmployeePayrollEarningDto>> GetEmployeePayrollEarningsNotLocked(string employeeId);
    Task<List<EmployeePayrollEarningDto>> GetByEmployeeIdAsync(string employeeId);
    Task<List<EmployeePayrollEarningDto>> GetByEmployeeIdAndIsNotLockedAsync(string employeeId);
    Task<List<EmployeePayrollEarningDto>> GetByEmployeeIdAndLastRunIdAsync(string employeeId);
    Task<List<EmployeePayrollEarningDto>> GetByPayrollRunIdAsync(int payrollRunId);
    Task<List<EmployeePayrollEarningDto>> GetByTaxCodeAsync(int taxCode);
    Task<List<EmployeePayrollEarningDto>> GetByPayrollEarningIdAsync(string payrollEarningId);
    Task<EmployeePayrollEarningDto> UpdateAsync(EmployeePayrollEarningUpdateDto employeePayrollEarningUpdateDto);
    Task LockEmployeePayrollEarningsAsync();
    Task RollOverEmployeePayrollEarningsAsync();
  }
}

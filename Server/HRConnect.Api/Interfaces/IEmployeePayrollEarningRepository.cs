namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models.Payroll.Earning;

  public interface IEmployeePayrollEarningRepository
  {
    Task<EmployeePayrollEarning> AddAsync(EmployeePayrollEarning employeePayrollEarning);
    Task<EmployeePayrollEarning?> CheckIfEmployeeEarningExistsForCurrentPayrun(string employeeId, string payrollEarningId, int payrollRunId);
    Task<List<EmployeePayrollEarning>> GetAllAsync();
    Task<List<EmployeePayrollEarning>> GetEmployeePayrollEarningsNotLocked(string employeeId);
    Task<List<EmployeePayrollEarning>> GetByEmployeeIdAsync(string employeeId);
    Task<List<EmployeePayrollEarning>> GetByEmployeeIdAndIsNotLockedAsync(string employeeId);
    Task<List<EmployeePayrollEarning>> GetByEmployeeIdAndLastRunIdAsync(string employeeId);
    Task<List<EmployeePayrollEarning>> GetByPayrollRunIdAsync(int payrollRunId);
    Task<List<EmployeePayrollEarning>> GetByTaxCodeAsync(int taxCode);
    Task<List<EmployeePayrollEarning>> GetByPayrollEarningIdAsync(string payrollEarningId);
    Task<EmployeePayrollEarning> UpdateAsync(EmployeePayrollEarning employeePayrollEarning);
    Task LockEmployeePayrollEarningsAsync(List<EmployeePayrollEarning> employeePayrollEarnings);
  }
}

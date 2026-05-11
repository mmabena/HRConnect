namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models.PayrollDeduction;

  public interface IEmployeeDeductionRepository
  {
    Task<EmployeeDeduction> AddAsync(EmployeeDeduction employeeDeduction);
    Task AddRangeAsync(List<EmployeeDeduction> employeeDeductions);
    Task<EmployeeDeduction?> CheckIfEmployeeDeductionExistsForCurrentPayrun(string employeeId, string deductionId, int payrollRunId);
    Task<List<EmployeeDeduction>> GetAllAsync();
    Task<List<EmployeeDeduction>> GetByEmployeeIdAsync(string employeeId);
    Task<List<EmployeeDeduction>> GetByEmployeeIdAndIsNotLockedAsync(string employeeId);
    Task<List<EmployeeDeduction>> GetByEmployeeIdAndLastRunIdAsync(string employeeId);
    Task<List<EmployeeDeduction>> GetByPayrollRunIdAsync(int payrollRunId);
    Task<List<EmployeeDeduction>> GetByDeductionIdAsync(string deductionId);
    Task<EmployeeDeduction> UpdateAsync(EmployeeDeduction employeeDeduction);
    Task UpdateRangeAsync(List<EmployeeDeduction> employeeDeductions);
    Task LockEmployeeDeductionsAsync(List<EmployeeDeduction> employeeDeductions);
  }
}

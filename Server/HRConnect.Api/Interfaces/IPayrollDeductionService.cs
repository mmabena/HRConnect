namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  public interface IPayrollDeductionService
  {
    // have a task to apply deductions
    Task<List<PayrollDeduction>> GetAllUifDeductions();
    Task<Employee?> DeductUifAsync(int employeeId);
    Task<Employee?> GetEmployeeByCodeAsync(string employeeCode);
    // Task<Employee?> GetEmployeeSalaryAsync(int employeeId);
  }
}
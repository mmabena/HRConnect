namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  public interface IPayrollDeductionService
  {
    Task<List<PayrollDeduction>> GetAllDeductionsAsync();
    Task<PayrollDeduction?> AddDeductionsAsync(int employeeId);
    Task<Employee?> GetEmployeeByCodeAsync(string employeeCode);
    Task<PayrollDeduction?> GetDeductionsByEmployeeIdAsync(int employeeId);

  }
}
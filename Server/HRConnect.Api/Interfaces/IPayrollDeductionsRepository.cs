namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  public interface IPayrollDeductionsRepository
  {
    Task<PayrollDeduction> AddDeductionsAsync(PayrollDeduction payrollDeductions);
    Task<IEnumerable<PayrollDeduction>> GetAllDeductionsAsync();
    Task<PayrollDeduction?> GetDeductionsByEmployeeIdAsync(string employeeId);
  }
}
namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  public interface IPayrollContributionsRepo
  {
    Task<PayrollDeduction> AddDeductionsAsync(PayrollDeduction payrollDeductions);
    Task<List<PayrollDeduction>> GetAllDeductionsAsync();
    Task<PayrollDeduction?> GetDeductionsByEmployeeIdAsync(int employeeId);
  }
}
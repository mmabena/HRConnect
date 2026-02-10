namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;

  public interface IPayrollContributionsRepo
  {
    Task<PayrollDeduction> AddUifDeductionAsync(decimal uifEmployee, decimal uifEmployer, decimal sdlAmount, int employeeId);
    Task<List<PayrollDeduction>> GetAllUifDeductionsAsync();
    Task<PayrollDeduction?> GetUifDeductionsByIdAsync(int id);
    Task<PayrollDeduction?> GetUifDeductionsByEmployeeIdAsync(int employeeId);
    // Task<PayrollDeduction?> GetUifDeductionsByEmployeeCodeAsync(string employeeCode);
  }
}
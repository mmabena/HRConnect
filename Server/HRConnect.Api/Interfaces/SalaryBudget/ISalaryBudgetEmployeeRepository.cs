namespace HRConnect.Api.Interfaces.SalaryBudget
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Models.SalaryBudget;
  public interface ISalaryBudgetEmployeeRepository
  {
    Task<List<SalaryBudgetEmployee>> GetAllBudgetEmployeesAsync();
    Task<SalaryBudgetEmployee?> GetBudgetEmployeeByIdAsync(int budgetEmployeeId);
    Task<SalaryBudgetEmployee> CreateBudgetEmployeeAsync(SalaryBudgetEmployee salaryBudget);
    Task<SalaryBudgetEmployee> UpdateBudgetEmployeeAsync(SalaryBudgetEmployee salaryBudget);
    Task<bool> DeleteBudgetEmployeeAsync(int budgetEmployeeId);
  }
}
namespace HRConnect.Api.Interfaces.SalaryBudget
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Models.SalaryBudget;
  public interface ISalaryBudgetEmployeeRepository
  {
    Task<List<SalaryBudgetEmployee>> GetAllBudgetEmployeesAsync();
    Task<SalaryBudgetEmployee?> GetBudgetEmployeeByIdAsync(int budgetEmployeeId);
    Task<SalaryBudgetEmployee> CreateBudgetEmployeeAsync(SalaryBudgetEmployee SalaryBudgetEmployee);
    Task<SalaryBudgetEmployee> UpdateBudgetEmployeeAsync(SalaryBudgetEmployee SalaryBudgetEmployee);
    Task<bool> DeleteBudgetEmployeeAsync(int budgetEmployeeId);
  }
}


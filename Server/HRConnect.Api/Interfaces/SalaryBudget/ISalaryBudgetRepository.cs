namespace HRConnect.Api.Interfaces.SalaryBudget
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Models.SalaryBudget;
    public interface ISalaryBudgetRepository
    {
      Task<List<SalaryBudget>> GetSalaryBudgetsAsync();
      Task<SalaryBudget?> GetSalaryBudgetByIdAsync(int salaryBudgetId);
      Task<SalaryBudget> CreateSalaryBudgetAsync(SalaryBudget salaryBudget);
      Task<SalaryBudget> UpdateEmployeeAsync(SalaryBudget salaryBudget);
      Task<bool> RemoveEmployeeAsync(int salaryBudgetEmployeeId);
      Task<bool> DeleteBudgetAsync (int salaryBudgetId);
        
    }
}
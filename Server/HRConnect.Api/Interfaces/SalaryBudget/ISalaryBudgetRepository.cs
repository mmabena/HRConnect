namespace HRConnect.Api.Interfaces.SalaryBudget
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Models.SalaryBudget;
  public interface ISalaryBudgetRepository
  {
    Task<List<SalaryBudget>> GetAllBudgetsAsync();
    Task<SalaryBudget?> GetBudgetByIdAsync(int salaryBudgetId);
    Task<SalaryBudget> CreateBudgetAsync(SalaryBudget salaryBudgetModel);
    Task<SalaryBudget> UpdateBudgetAsync(SalaryBudget salaryBudgetModel);
    Task<bool> ArchiveBudgetAsync(int salaryBudgetId);

  }
}
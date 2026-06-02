namespace HRConnect.Api.Interfaces.SalaryBudget
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs.SalaryBudget;
  public interface ISalaryBudgetService
  {
    Task<List<SalaryBudgetDto>> GetAllBudgetsAsync();
    Task<SalaryBudgetDto?> GetBudgetByIdAsync(int salaryBudgetId);
    Task<SalaryBudgetDto> CreateBudgetAsync(CreateSalaryBudgetDto salaryBudgetDto);
    Task<SalaryBudgetDto> UpdateBudgetAsync(UpdateSalaryBudgetDto updateSalaryBudgetDto);
    Task<bool> DeleteBudgetAsync(int salaryBudgetId);
  }
}
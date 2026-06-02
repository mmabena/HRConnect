namespace HRConnect.Api.Interfaces.SalaryBudget
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
   using HRConnect.Api.DTOs.SalaryBudgetEmployee;
  public interface ISalaryBudgetEmployeeService
  {
    Task<List<SalaryBudgetEmployeeDto>> GetAllBudgetEmployeesAsync();
    Task<SalaryBudgetEmployeeDto?> GetBudgetEmployeeByIdAsync(int budgetEmployeeId);
    Task<SalaryBudgetEmployeeDto> CreateBudgetEmployeeAsync(CreateBudgetEmployeeDto createBudgetEmployeeDto);
    Task<SalaryBudgetEmployeeDto> UpdateBudgetEmployeeAsync(UpdateBudgetEmployeeDto updateDto);
    Task<bool> DeleteBudgetEmployeeAsync(int budgetEmployeeId);
  }
}
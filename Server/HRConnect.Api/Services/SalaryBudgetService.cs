namespace HRConnect.Api.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.Dtos.SalaryBudget;
  using HRConnect.Api.Mappers.SalaryBudget;
  using Microsoft.EntityFrameworkCore;
  using HRConnect.Api.Interfaces.SalaryBudget;

  /// <summary>
   /// Salary budget business logic/rules
   /// </summary>
  public class SalaryBudgetService : ISalaryBudgetService
  {
    private readonly ISalaryBudgetRepository _salaryBudgetRepo;

    public SalaryBudgetService(ISalaryBudgetRepository salaryBudgetRepo)
    {
        _salaryBudgetRepo = salaryBudgetRepo;
    }

    public async Task<List<SalaryBudgetDto>> GetAllBudgetsAsync()
    {
        var budgets = await _salaryBudgetRepo.GetAllBudgetsAsync();
        return budgets
        .Select(b => b.ToSalaryBudgetDto())
        .ToList();
    }
    
     /// <summary>
     /// Gets Salary Budgets by their Id
     /// </summary>
     /// <param name="salaryBudgetId"></param>
     /// <returns></returns>
    public async Task<SalaryBudgetDto?> GetBudgetByIdAsync(int salaryBudgetId)
    {
      var budget = await _salaryBudgetRepo.GetBudgetByIdAsync(salaryBudgetId);

      return budget?.ToSalaryBudgetDto();
    }
    

     public async Task<SalaryBudgetDto> CreateBudgetAsync(CreateSalaryBudgetDto createBudgetDto)
    {
      var salaryBudget = createBudgetDto.ToSalaryBudgetCreateDto();
      await _salaryBudgetRepo.CreateBudgetAsync(salaryBudget);
      return salaryBudget.ToSalaryBudgetDto();
    }

    public async Task<SalaryBudgetDto> UpdateBudgetAsync(UpdateSalaryBudgetDto updateBudgetDto)
    {
      
    }

     public async Task<bool> ArchiveBudgetAsync (int salaryBudgetId)
    {
      var budget = await _context.SalaryBudgets
          .FindAsync(salaryBudgetId);

        if(budget == null)
      {
        return false;
      }

       budget.Status = SalaryBudgetStatus.Archived;
       budget.ArchivedDate = DateTime.UtcNow;

       await _context.SaveChangesAsync();

       return true;
    }

  }
}
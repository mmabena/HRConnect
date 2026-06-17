namespace HRConnect.Api.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.Dtos.SalaryBudget;
  using Microsoft.EntityFrameworkCore;
  using HRConnect.Api.Interfaces.SalaryBudget;
  public class SalaryBudgetService : ISalaryBudgetService
  {

    private readonly ISalaryBudgetRepository _salaryBudgetRepo;

    public SalaryBudgetService(ISalaryBudgetRepository salaryBudgetRepo)
    {
        _salaryBudgetRepo = salaryBudgetRepo;
    }

    public async Task<List<SalaryBudgetDto>> GetAllBudgetsAsync()
    {
      
      
    }

    public async Task<SalaryBudgetDto?> GetBudgetByIdAsync(int salaryBudgetId)
    {
      
    }

     public async Task<SalaryBudgetDto> CreateBudgetAsync(CreateSalaryBudgetDto createBudgetDto)
    {
      
    }

    public async Task<SalaryBudgetDto> UpdateBudgetAsync(UpdateSalaryBudgetDto updateBudgetDto)
    {
      
    }

    public async Task<bool> ArchiveBudgetAsync(int salaryBudgetId)
    {
      
    }






  }
}
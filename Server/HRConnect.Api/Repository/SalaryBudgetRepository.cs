namespace HRConnect.Api.Repository
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.SalaryBudget;
  using HRConnect.Api.Interfaces.SalaryBudget;
  using Microsoft.EntityFrameworkCore;
  using HRConnect.Api.Data;

   /// <summary>
   /// Database access/communicates to the database 
   /// </summary>
    public class SalaryBudgetRepository : ISalaryBudgetRepository
    {
      private readonly ApplicationDBContext _context;

      public SalaryBudgetRepository(ApplicationDBContext context)
    {
        _context = context;
    }

     public async Task<List<SalaryBudget>> GetAllBudgetsAsync()
    {
       return await _context.SalaryBudgets
              .Include(sb => sb.Employees)
              .ToListAsync();
    }
     public async Task<SalaryBudget?> GetBudgetByIdAsync(int salaryBudgetId)
    {
      return await _context.SalaryBudgets
              .Include(sb => sb.Employees)
              .FirstOrDefaultAsync(
                sb => sb.SalaryBudgetId ==salaryBudgetId);
    }

    public async Task<SalaryBudget> CreateBudgetAsync(SalaryBudget salaryBudgetModel)
    {
        await _context.SalaryBudgets.AddAsync(salaryBudgetModel);
        await _context.SaveChangesAsync();
        return salaryBudgetModel;
    }

    public async Task<SalaryBudget> UpdateBudgetAsync(SalaryBudget salaryBudgetModel)
    {
       _context.SalaryBudgets.Update(salaryBudgetModel);
       await _context.SaveChangesAsync();
       return salaryBudgetModel;
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
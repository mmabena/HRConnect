namespace HRConnect.Api.Repository
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Models.SalaryBudget;
  using HRConnect.Api.Interfaces.SalaryBudget;
  using Microsoft.EntityFrameworkCore;
  using HRConnect.Api.Data;

    public class SalaryBudgetRepository : ISalaryBudgetRepository
    {
      private readonly ApplicationDBContext _context;

      public SalaryBudgetRepository(ApplicationDBContext context)
    {
        _context = context;
    }

     public async Task<List<SalaryBudget>> GetAllSalaryBudgetsAsync()
    {
       return await _context.SalaryBudgets
              .Include(sb => sb.Employees)
              .Include(sb => sb.Position)
              .Include(sb => sb.JobGrade)
              .ToListAsync();

    }

     public async Task<SalaryBudget?> GetBudgetByIdAsync(int salaryBudgetId)
    {
      return await _context.SalaryBudgets
              .Include(sb => sb.Employees)
              .Include(sb => sb.Position)
              .Include(sb => sb.JobGrade)
              .FirstOrDefaultAsync(sb => sb.SalaryBudgetId ==salaryBudgetId);
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

    public async Task<bool> DeleteBudgetAsync (int salaryBudgetId)
    {
       _context.SalaryBudgets.Remove(salaryBudgetId);
       await _context.SaveChangesAsync();
       return salaryBudgetId;
    }
        
    }
}
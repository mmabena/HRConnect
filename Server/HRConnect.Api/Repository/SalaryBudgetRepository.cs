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
    /// <summary>
    /// Retrieves all Budgets from the database.
    /// </summary>
    /// <returns></returns>
    public async Task<List<SalaryBudget>> GetAllBudgetsAsync()
    {
      return await _context.SalaryBudgets
             .ToListAsync();
    }

    /// <summary>
    /// Retrieves Salary Budgets by their status 
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public async Task<List<SalaryBudget>> GetBudgetsByStatusAsync(SalaryBudgetStatus status)
    {
      return await _context.SalaryBudgets
             .Where(sb => sb.Status == status)
             .ToListAsync();
    }

    /// <summary>
    /// Gets Salary Budgets by their Id
    /// </summary>
    /// <param name="salaryBudgetId"></param>
    /// <returns></returns>

    public async Task<SalaryBudget?> GetBudgetByIdAsync(int salaryBudgetId)
    {
      return await _context.SalaryBudgets
              .Include(sb => sb.Employees)
              .FirstOrDefaultAsync(
                sb => sb.SalaryBudgetId == salaryBudgetId);
    }
    /// <summary>
    /// Creates a new SalaryBudget in the database.
    /// </summary>
    /// <param name="salaryBudgetModel"></param>
    /// <returns></returns>
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



  }
}
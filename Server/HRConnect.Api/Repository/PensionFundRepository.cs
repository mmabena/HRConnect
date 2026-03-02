namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Data;
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public class PensionFundRepository(ApplicationDBContext context) : IPensionRepository
  {

    public async Task<IEnumerable<PensionFund>> GetPensionFundsAsync()
    {
      return await context.PensionFunds.ToListAsync();
    }

    public async Task<PensionFund?> GetPensionFundByIdAsync(int id)
    {
      return await context.PensionFunds
                          .FirstOrDefaultAsync(f => f.PensionFundId == id);
    }

    public async Task AddPensionFundAsync(PensionFund fund)
    {
      _ = await context.PensionFunds.AddAsync(fund);
      _ = await context.SaveChangesAsync();
    }

    public async Task UpdatePensionFundAsync(PensionFund fund)
    {
      _ = context.PensionFunds.Update(fund);
      _ = await context.SaveChangesAsync();
    }

<<<<<<< HEAD
    // Add or update PensionFund for an employee
    public async Task AddOrUpdatePensionFundAsync(PensionFund fund)
    {
      PensionFund? existingFund = await context.PensionFunds.
        FirstOrDefaultAsync(f => f.EmployeeId == fund.EmployeeId);

      if (existingFund == null)
      {
        _ = await context.PensionFunds.AddAsync(fund);
      }
      else
      {
        existingFund.PensionOptionId = fund.PensionOptionId;
        existingFund.ContributionAmount = fund.ContributionAmount;
        _ = context.PensionFunds.Update(existingFund);
      }
    }

    public async Task SaveChangesAsync()
    {
      _ = await context.SaveChangesAsync();
    }

    // ============================
    // Pension Options
    // ============================

=======
>>>>>>> 82a5d8a54eb2645c4a2a43003643340a03574021
    public async Task<IEnumerable<PensionOption>> GetPensionOptionsAsync()
    {
      return await context.PensionOptions.ToListAsync();
    }

    public async Task<PensionOption?> GetPensionOptionByIdAsync(int id)
    {
      return await context.PensionOptions
                          .FirstOrDefaultAsync(o => o.PensionOptionId == id);
    }

    public async Task AddPensionOptionAsync(PensionOption pensionoption)
    {
      _ = await context.PensionOptions.AddAsync(pensionoption);
      _ = await context.SaveChangesAsync();
    }

    public async Task UpdatePensionOptionAsync(PensionOption pensionoption)
    {
      _ = context.PensionOptions.Update(pensionoption);
      _ = await context.SaveChangesAsync();
    }

    // ============================
    // Employees
    // ============================

    // FIX: EmployeeId is a string, not int
    public async Task<Employee?> GetEmployeeByIdAsync(string id)
    {
      return await context.Employees
                          .FirstOrDefaultAsync(e => e.EmployeeId == id);
    }
  }
}


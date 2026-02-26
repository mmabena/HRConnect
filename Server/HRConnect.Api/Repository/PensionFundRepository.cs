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
    // ============================
    // Pension Funds
    // ============================

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

    // ============================
    // Pension Options
    // ============================

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

  }
}



namespace HRConnect.Api.Repository
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  public class PensionDeductionRepository(ApplicationDBContext context) : IPensionDeductionRepository
  {
    // ============================
    // Pension Funds (for deduction)
    // ============================

    public async Task<List<PensionFund>> GetAllPensionFundsWithOptionsAsync()
    {
      return await context.PensionFunds
          .Include(p => p.PensionOptions)
          .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
      _ = await context.SaveChangesAsync();
    }
  }
}
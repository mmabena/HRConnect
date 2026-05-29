

namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Data;
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public class PensionFundRepository(ApplicationDBContext context) : IPensionFundRepository
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

    public async Task AddOrUpdatePensionFundAsync(PensionFund fund)
    {
      PensionFund? existingFund = await context.PensionFunds
          .FirstOrDefaultAsync(f => f.EmployeeId == fund.EmployeeId);

      if (existingFund == null)
      {
        _ = await context.PensionFunds.AddAsync(fund);
      }
      else
      {
        existingFund.PensionOptionId = fund.PensionOptionId;
        existingFund.ContributionAmount = fund.ContributionAmount;
        existingFund.MonthlySalary = fund.MonthlySalary;
        existingFund.ContributionPercentage = fund.ContributionPercentage;
        existingFund.TaxCode = fund.TaxCode;

        _ = context.PensionFunds.Update(existingFund);
      }
    }

    public async Task SaveChangesAsync()
    {
      _ = await context.SaveChangesAsync();
    }
  }
}
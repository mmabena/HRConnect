

namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Data;
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using System.Threading;
  public class PensionFundRepository(ApplicationDBContext context) : IPensionFundRepository
  {
    public async Task<IEnumerable<PensionFund>> GetPensionFundsAsync(CancellationToken cancellationToken)
    {
      return await context.PensionFunds.ToListAsync(cancellationToken);
    }

    public async Task<PensionFund?> GetPensionFundByIdAsync(int id, CancellationToken cancellationToken)
    {
      return await context.PensionFunds
                          .FirstOrDefaultAsync(f => f.PensionFundId == id, cancellationToken);
    }

    public async Task AddPensionFundAsync(PensionFund fund, CancellationToken cancellationToken)
    {
      _ = await context.PensionFunds.AddAsync(fund, cancellationToken);
      _ = await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePensionFundAsync(PensionFund fund, CancellationToken cancellationToken)
    {
      _ = context.PensionFunds.Update(fund);
      _ = await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddOrUpdatePensionFundAsync(PensionFund fund, CancellationToken cancellationToken)
    {
      PensionFund? existingFund = await context.PensionFunds
          .FirstOrDefaultAsync(f => f.EmployeeId == fund.EmployeeId, cancellationToken);

      if (existingFund == null)
      {
        _ = await context.PensionFunds.AddAsync(fund, cancellationToken);
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

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
      _ = await context.SaveChangesAsync(cancellationToken);
    }
  }
}
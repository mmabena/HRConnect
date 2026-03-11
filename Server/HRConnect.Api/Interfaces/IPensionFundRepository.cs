namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using System.Threading;

  public interface IPensionFundRepository
  {
    // Pension Funds
    Task<IEnumerable<PensionFund>> GetPensionFundsAsync(CancellationToken cancellationToken);

    Task<PensionFund?> GetPensionFundByIdAsync(int id, CancellationToken cancellationToken);

    Task AddPensionFundAsync(PensionFund fund, CancellationToken cancellationToken);

    Task UpdatePensionFundAsync(PensionFund fund, CancellationToken cancellationToken);

    Task AddOrUpdatePensionFundAsync(PensionFund fund, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
  }
}
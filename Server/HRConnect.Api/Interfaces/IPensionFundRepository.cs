namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public interface IPensionFundRepository
  {
    // Pension Funds
    Task<IEnumerable<PensionFund>> GetPensionFundsAsync();

    Task<PensionFund?> GetPensionFundByIdAsync(int id);

    Task AddPensionFundAsync(PensionFund fund);

    Task UpdatePensionFundAsync(PensionFund fund);

    Task AddOrUpdatePensionFundAsync(PensionFund fund);

    Task SaveChangesAsync();
  }
}
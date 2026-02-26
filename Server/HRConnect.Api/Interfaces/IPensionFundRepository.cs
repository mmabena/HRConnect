
namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;

  public interface IPensionRepository
  {
    // Pension Funds
    Task<IEnumerable<PensionFund>> GetPensionFundsAsync();
    Task<PensionFund?> GetPensionFundByIdAsync(int id);
    Task AddPensionFundAsync(PensionFund fund);
    Task UpdatePensionFundAsync(PensionFund fund);

    // Pension Options
    Task<IEnumerable<PensionOption>> GetPensionOptionsAsync();
    Task<PensionOption?> GetPensionOptionByIdAsync(int id);
    Task AddPensionOptionAsync(PensionOption pensionoption);
    Task UpdatePensionOptionAsync(PensionOption pensionoption);
  }
}


namespace HRConnect.Api.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Models;

  public interface IPensionOptionRepository
  {
    Task<decimal> GetPensionOptionPercentageByIdAsync(int id);
    Task<IEnumerable<PensionOption>> GetPensionOptionsAsync();

    Task<PensionOption?> GetPensionOptionByIdAsync(int id);

    Task<ServiceResult> AddPensionOptionAsync(PensionOption pensionOption);

    Task<ServiceResult> UpdatePensionOptionAsync(PensionOption pensionOption);
  }
}


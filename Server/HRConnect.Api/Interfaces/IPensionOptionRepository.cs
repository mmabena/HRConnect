namespace HRConnect.Api.Interfaces
{
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using HRConnect.Api.Models;

  public interface IPensionOptionRepository
  {
    Task<IEnumerable<PensionOption>> GetPensionOptionsAsync(CancellationToken cancellationToken);

    Task<PensionOption?> GetPensionOptionByIdAsync(int id, CancellationToken cancellationToken);

    Task<ServiceResult> AddPensionOptionAsync(PensionOption pensionOption, CancellationToken cancellationToken);

    Task<ServiceResult> UpdatePensionOptionAsync(PensionOption pensionOption, CancellationToken cancellationToken);
  }
}



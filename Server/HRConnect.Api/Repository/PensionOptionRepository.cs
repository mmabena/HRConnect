namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Data;
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;

  public class PensionOptionRepository(ApplicationDBContext context) : IPensionOptionRepository
  {
    public async Task<IEnumerable<PensionOption>> GetPensionOptionsAsync(CancellationToken cancellationToken)
    {
      return await context.PensionOptions.ToListAsync(cancellationToken);
    }

    public async Task<PensionOption?> GetPensionOptionByIdAsync(int id, CancellationToken cancellationToken)
    {
      return await context.PensionOptions
          .FirstOrDefaultAsync(o => o.PensionOptionId == id, cancellationToken);
    }

    public async Task<ServiceResult> AddPensionOptionAsync(PensionOption pensionOption, CancellationToken cancellationToken)
    {
      _ = await context.PensionOptions.AddAsync(pensionOption, cancellationToken);
      _ = await context.SaveChangesAsync(cancellationToken);

      return ServiceResult.Success("Pension option added successfully.");
    }

    public async Task<ServiceResult> UpdatePensionOptionAsync(PensionOption pensionOption, CancellationToken cancellationToken)
    {
      _ = context.PensionOptions.Update(pensionOption);
      _ = await context.SaveChangesAsync(cancellationToken);

      return ServiceResult.Success("Pension option updated successfully.");
    }
  }
}
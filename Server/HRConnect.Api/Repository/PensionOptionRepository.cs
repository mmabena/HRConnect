namespace HRConnect.Api.Repository
{
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;
  using System.Threading;

  public class PensionOptionRepository(ApplicationDBContext context) : IPensionOptionRepository
  {
    private readonly ApplicationDBContext _context = context;
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

    ///<summary>
    ///Get pension option by id
    ///</summary>
    ///<param name="id">Pension Option Id</param>
    ///<returns>
    ///Pension option with the specified id
    ///</returns>
    public async Task<decimal> GetPensionOptionPercentageByIdAsync(int id)
    {
      return await _context.PensionOptions.Where(po => po.PensionOptionId == id)
        .Select(po => po.ContributionPercentage).FirstOrDefaultAsync();
    }


    public async Task<ServiceResult> DeleteAllPensionOptionsAsync(CancellationToken cancellationToken)
    {
      _context.PensionOptions.RemoveRange(_context.PensionOptions);
      await _context.SaveChangesAsync(cancellationToken);
      return ServiceResult.Success("All options deleted.");
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
      await _context.SaveChangesAsync(cancellationToken);
    }



  }
}
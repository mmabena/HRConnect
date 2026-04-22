namespace HRConnect.Api.Repository
{
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;

  public class PensionOptionRepository : IPensionOptionRepository
  {

    private readonly ApplicationDBContext _context;
    public PensionOptionRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<PensionOption>> GetPensionOptionsAsync()
    {
      return await _context.PensionOptions.ToListAsync();
    }

    public async Task<PensionOption?> GetPensionOptionByIdAsync(int id)
    {
      return await _context.PensionOptions
          .FirstOrDefaultAsync(o => o.PensionOptionId == id);
    }

    public async Task<ServiceResult> AddPensionOptionAsync(PensionOption pensionOption)
    {
      _ = await _context.PensionOptions.AddAsync(pensionOption);
      _ = await _context.SaveChangesAsync();

      return ServiceResult.Success("Pension option added successfully.");
    }

    public async Task<ServiceResult> UpdatePensionOptionAsync(PensionOption pensionOption)
    {
      _ = _context.PensionOptions.Update(pensionOption);
      _ = await _context.SaveChangesAsync();

      return ServiceResult.Success("Pension option updated successfully.");
    }

    ///<summary>
    ///Get pension option by ud
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
  }
}
namespace HRConnect.Api.Repository
{
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;

  public class PensionOptionRepository(ApplicationDBContext context) : IPensionOptionRepository
  {
    private readonly ApplicationDBContext _context = context;

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

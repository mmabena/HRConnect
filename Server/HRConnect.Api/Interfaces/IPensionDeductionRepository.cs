

namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  // Handles data access for PensionDeduction logic.
  // Repository only deals with fetching PensionFund records and saving updates.
  public interface IPensionDeductionRepository
  {
    // Returns all PensionFund records including their selected PensionOption.
    Task<List<PensionFund>> GetAllPensionFundsWithOptionsAsync();
    Task SaveChangesAsync();
  }
}
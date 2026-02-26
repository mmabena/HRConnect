
namespace HRConnect.Api.Interfaces
{
  using System.Threading.Tasks;

  /// <summary>
  /// Service interface for managing pension deductions for all employees.
  /// Handles calculation and updating of ContributionAmount in the PensionFund table.
  /// </summary>
  public interface IPensionFundDeductionService
  {
    /// <summary>
    /// Calculates the pension deduction (ContributionAmount) for all employees
    /// and updates the PensionFund table.
    /// Should only run on the 26th of each month.
    /// </summary>
    Task ProcessMonthlyPensionDeductionsAsync();
  }
}
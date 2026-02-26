
namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using System;
  using System.Threading.Tasks;

  // calculates and updates pension deductions (ContributionAmount) for employees.

  public class PensionFundDeductionService(IPensionDeductionRepository repository) : IPensionFundDeductionService
  {
    private readonly IPensionDeductionRepository _repository = repository;

    
    // Only runs on the 26th of each month. 
    public async Task ProcessMonthlyPensionDeductionsAsync()
    {
      DateTime today = DateTime.Today;

      // Only process on the 26th
      if (today.Day != 26)
      {
        return;
      }

      // Get all PensionFund records with their PensionOptions
      List<PensionFund> pensionFunds =
          await _repository.GetAllPensionFundsWithOptionsAsync();

      foreach (PensionFund fund in pensionFunds)
      {
        if (fund.PensionOptions == null)
        {
          continue;
        }

        // Calculate contribution using monthly salary and selected option
        decimal contributionAmount = CalculatePensionDeduction(
            fund.MonthlySalary,
            fund.PensionOptions.ContributionPercentage
        );

        
        fund.ContributionAmount = contributionAmount;
      }

      
      await _repository.SaveChangesAsync();
    }

    // Calculates the pension deduction based on monthly salary and contribution percentage.
    private static decimal CalculatePensionDeduction(decimal monthlySalary, decimal contributionPercentage)
    {
      return monthlySalary * (contributionPercentage / 100);
    }
  }
}


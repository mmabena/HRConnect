namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  // Primary constructor style
  public class PensionFundService(IPensionRepository repo) : IPensionFundService
  {
    // ============================
    // Pension Funds
    // ============================

    public async Task<IEnumerable<PensionFund>> GetPensionFundsAsync()
    {
      return await repo.GetPensionFundsAsync();
    }

    public async Task<PensionFund?> GetPensionFundByIdAsync(int id)
    {
      return await repo.GetPensionFundByIdAsync(id);
    }

    public async Task<ServiceResult> AddPensionFundAsync(PensionFund fund)
    {
      await repo.AddPensionFundAsync(fund);
      return ServiceResult.Success("Fund added successfully.");
    }

    public async Task<ServiceResult> UpdatePensionFundAsync(PensionFund fund)
    {
      await repo.UpdatePensionFundAsync(fund);
      return ServiceResult.Success("Fund updated successfully.");
    }

    // ============================
    // Pension Options
    // ============================

    public async Task<IEnumerable<PensionOption>> GetPensionOptionsAsync()
    {
      return await repo.GetPensionOptionsAsync();
    }

    public async Task<PensionOption?> GetPensionOptionByIdAsync(int id)
    {
      return await repo.GetPensionOptionByIdAsync(id);
    }

    public async Task<ServiceResult> AddPensionOptionAsync(PensionOption pensionoption)
    {
      if (pensionoption.ContributionPercentage is < 0 or > 15)
      {
        return ServiceResult.Failure("Percentage must be between 0 and 15.");
      }

      IEnumerable<PensionOption> existingOptions = await repo.GetPensionOptionsAsync();

      foreach (PensionOption option in existingOptions)
      {
        if (option.ContributionPercentage == pensionoption.ContributionPercentage)
        {
          return ServiceResult.Failure("An option with this percentage already exists.");
        }
      }

      await repo.AddPensionOptionAsync(pensionoption);
      return ServiceResult.Success("Option added successfully.");
    }

    public async Task<ServiceResult> UpdatePensionOptionAsync(PensionOption pensionoption)
    {
      if (pensionoption.ContributionPercentage is < 0 or > 15)
      {
        return ServiceResult.Failure("Percentage must be between 0 and 15.");
      }

      await repo.UpdatePensionOptionAsync(pensionoption);
      return ServiceResult.Success("Option updated successfully.");
    }

    // ============================
    // Pension Deduction
    // ============================

    public decimal CalculatePensionDeduction(decimal monthlySalary, PensionOption pensionoption)
    {
      return monthlySalary * (pensionoption.ContributionPercentage / 100);
    }

    // ============================
    // Record Pension Option Selection
    // ============================

    public async Task<ServiceResult> RecordEmployeePensionSelectionAsync(string employeeId, int pensionOptionId)
    {
      Employee? employee = await repo.GetEmployeeByIdAsync(employeeId);
      PensionOption? option = await repo.GetPensionOptionByIdAsync(pensionOptionId);

      if (employee == null || option == null)
      {
        return ServiceResult.Failure("Employee or Pension Option not found.");
      }

      if (employee.EmploymentStatus != EmploymentStatus.Permanent)
      {
        return ServiceResult.Failure("Only permanent employees may select a pension option.");
      }

      decimal salary = employee.MonthlySalary ?? 0m;
      decimal contributionAmount = salary * (option.ContributionPercentage / 100);


      PensionFund fund = new()
      {
        EmployeeId = employee.EmployeeId,
        EmployeeName = employee.Name,
        PensionOptionId = option.PensionOptionId,
        MonthlySalary = employee.MonthlySalary ?? 0m,          // snapshot salary
        ContributionPercentage = option.ContributionPercentage, // snapshot percentage
        ContributionAmount = contributionAmount,
        TaxCode = 4001 // default tax code
      };

      await repo.AddOrUpdatePensionFundAsync(fund);
      await repo.SaveChangesAsync();

      return ServiceResult.Success("Pension option recorded and contribution calculated.");
    }


  }
}


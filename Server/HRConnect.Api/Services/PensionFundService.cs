namespace HRConnect.Api.Services
{
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;

  public class PensionFundService(
      IPensionFundRepository fundRepo,
      IPensionOptionRepository optionRepo,
      IEmployeePensionRepository employeeRepo
  ) : IPensionFundService
  {

    // Pension Funds
    public async Task<IEnumerable<PensionFund>> GetPensionFundsAsync(CancellationToken cancellationToken)
    {
      return await fundRepo.GetPensionFundsAsync(cancellationToken);
    }

    public async Task<PensionFund?> GetPensionFundByIdAsync(int id, CancellationToken cancellationToken)
    {
      return await fundRepo.GetPensionFundByIdAsync(id, cancellationToken);
    }

    public async Task<ServiceResult> AddPensionFundAsync(PensionFund fund, CancellationToken cancellationToken)
    {
      await fundRepo.AddPensionFundAsync(fund, cancellationToken);
      await fundRepo.SaveChangesAsync(cancellationToken);

      return ServiceResult.Success("Fund added successfully.");
    }

    public async Task<ServiceResult> UpdatePensionFundAsync(PensionFund fund, CancellationToken cancellationToken)
    {
      await fundRepo.UpdatePensionFundAsync(fund, cancellationToken);
      await fundRepo.SaveChangesAsync(cancellationToken);

      return ServiceResult.Success("Fund updated successfully.");
    }

    // Pension Options
    public async Task<IEnumerable<PensionOption>> GetPensionOptionsAsync(CancellationToken cancellationToken)
    {
      return await optionRepo.GetPensionOptionsAsync(cancellationToken);
    }

    public async Task<PensionOption?> GetPensionOptionByIdAsync(int id, CancellationToken cancellationToken)
    {
      return await optionRepo.GetPensionOptionByIdAsync(id, cancellationToken);
    }

    public async Task<ServiceResult> AddPensionOptionAsync(PensionOption pensionOption, CancellationToken cancellationToken)
    {
      if (pensionOption.ContributionPercentage is < 0 or > 15)
      {
        return ServiceResult.Failure("Percentage must be between 0 and 15.");
      }

      IEnumerable<PensionOption> existingOptions = await optionRepo.GetPensionOptionsAsync(cancellationToken);

      foreach (PensionOption option in existingOptions)
      {
        if (option.ContributionPercentage == pensionOption.ContributionPercentage)
        {
          return ServiceResult.Failure("An option with this percentage already exists.");
        }
      }

      return await optionRepo.AddPensionOptionAsync(pensionOption, cancellationToken);
    }

    public async Task<ServiceResult> UpdatePensionOptionAsync(PensionOption pensionOption, CancellationToken cancellationToken)
    {
      return pensionOption.ContributionPercentage is < 0 or > 15
        ? ServiceResult.Failure("Percentage must be between 0 and 15.")
        : await optionRepo.UpdatePensionOptionAsync(pensionOption, cancellationToken);
    }

    // Pension Deduction
    public decimal CalculatePensionDeduction(decimal monthlySalary, PensionOption pensionOption)
    {
      return monthlySalary * (pensionOption.ContributionPercentage / 100);
    }

    // Employee Pension Selection
    public async Task<ServiceResult> RecordEmployeePensionSelectionAsync(
        string employeeId,
        int pensionOptionId,
        CancellationToken cancellationToken)
    {
      Employee? employee = await employeeRepo.GetEmployeeByIdAsync(employeeId, cancellationToken);
      PensionOption? option = await optionRepo.GetPensionOptionByIdAsync(pensionOptionId, cancellationToken);

      if (employee == null || option == null)
      {
        return ServiceResult.Failure("Employee or Pension Option not found.");
      }

      if (employee.EmploymentStatus != EmploymentStatus.Permanent)
      {
        return ServiceResult.Failure("Only permanent employees may select a pension option.");
      }

      PensionFund? fundRecord = await fundRepo.GetPensionFundByIdAsync(1, cancellationToken);

      if (fundRecord == null)
      {
        return ServiceResult.Failure("No pension fund available.");
      }

      employee.PensionOptionId = option.PensionOptionId;

      decimal salary = employee.MonthlySalary ?? 0m;
      decimal contributionAmount = salary * (option.ContributionPercentage / 100);

      PensionFund fund = new()
      {
        EmployeeId = employee.EmployeeId,
        EmployeeName = employee.Name,
        PensionOptionId = option.PensionOptionId,
        MonthlySalary = salary,
        ContributionPercentage = option.ContributionPercentage,
        ContributionAmount = contributionAmount,
        TaxCode = 4001
      };

      await fundRepo.AddOrUpdatePensionFundAsync(fund, cancellationToken);
      await fundRepo.SaveChangesAsync(cancellationToken);

      return ServiceResult.Success("Pension option selected and employee updated.");
    }
  }
}
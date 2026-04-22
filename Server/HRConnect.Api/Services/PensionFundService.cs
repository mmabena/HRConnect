namespace HRConnect.Api.Services
{
  using System.Collections.Generic;
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
    public async Task<IEnumerable<PensionFund>> GetPensionFundsAsync()
    {
      return await fundRepo.GetPensionFundsAsync();
    }

    public async Task<PensionFund?> GetPensionFundByIdAsync(int id)
    {
      return await fundRepo.GetPensionFundByIdAsync(id);
    }

    public async Task<ServiceResult> AddPensionFundAsync(PensionFund fund)
    {
      await fundRepo.AddPensionFundAsync(fund);
      await fundRepo.SaveChangesAsync();

      return ServiceResult.Success("Fund added successfully.");
    }

    public async Task<ServiceResult> UpdatePensionFundAsync(PensionFund fund)
    {
      await fundRepo.UpdatePensionFundAsync(fund);
      await fundRepo.SaveChangesAsync();

      return ServiceResult.Success("Fund updated successfully.");
    }

    // Pension Options
    public async Task<IEnumerable<PensionOption>> GetPensionOptionsAsync()
    {
      return await optionRepo.GetPensionOptionsAsync();
    }

    public async Task<PensionOption?> GetPensionOptionByIdAsync(int id)
    {
      return await optionRepo.GetPensionOptionByIdAsync(id);
    }

    public async Task<ServiceResult> AddPensionOptionAsync(PensionOption pensionOption)
    {
      if (pensionOption.ContributionPercentage is < 0 or > 15)
      {
        return ServiceResult.Failure("Percentage must be between 0 and 15.");
      }

      IEnumerable<PensionOption> existingOptions = await optionRepo.GetPensionOptionsAsync();

      foreach (PensionOption option in existingOptions)
      {
        if (option.ContributionPercentage == pensionOption.ContributionPercentage)
        {
          return ServiceResult.Failure("An option with this percentage already exists.");
        }
      }

      return await optionRepo.AddPensionOptionAsync(pensionOption);
    }

    public async Task<ServiceResult> UpdatePensionOptionAsync(PensionOption pensionOption)
    {
      return pensionOption.ContributionPercentage is < 0 or > 15
        ? ServiceResult.Failure("Percentage must be between 0 and 15.")
        : await optionRepo.UpdatePensionOptionAsync(pensionOption);
    }

    // Pension Deduction
    public decimal CalculatePensionDeduction(decimal monthlySalary, PensionOption pensionOption)
    {
      return monthlySalary * (pensionOption.ContributionPercentage / 100);
    }

    // Employee Pension Selection
    public async Task<ServiceResult> RecordEmployeePensionSelectionAsync(
     string employeeId,
     int PensionOptionId)
    {
      Employee? employee = await employeeRepo.GetEmployeeByIdAsync(employeeId);
      PensionOption? option = await optionRepo.GetPensionOptionByIdAsync(PensionOptionId);

      if (employee == null || option == null)
        return ServiceResult.Failure("Employee or Pension Option not found.");

      if (employee.EmploymentStatus != EmploymentStatus.Permanent)
        return ServiceResult.Failure("Only permanent employees may select a pension option.");

      // Update employee with chosen option
      employee.PensionOptionId = option.PensionOptionId;

      decimal salary = employee.MonthlySalary;
      decimal contributionAmount = salary * (option.ContributionPercentage / 100);

      // Create a new PensionFund record automatically
      PensionFund fund = new()
      {
        EmployeeId = employee.EmployeeId,
        EmployeeName = employee.Name,
        PensionOptionId = option.PensionOptionId,
        MonthlySalary = salary,
        ContributionPercentage = option.ContributionPercentage,
        ContributionAmount = contributionAmount,
        TaxCode = 4001 // or derive dynamically
      };

      await fundRepo.AddOrUpdatePensionFundAsync(fund);
      await fundRepo.SaveChangesAsync();

      return ServiceResult.Success("Pension option selected and pension fund created.");
    }

  }
}
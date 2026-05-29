namespace HRConnect.Api.Services
{
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs;
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

    public async Task<ServiceResult> CreatePensionFundAsync(CreatePensionFundDto dto, CancellationToken cancellationToken)
    {
      // Check if an active fund already exists
      var existingFunds = await fundRepo.GetPensionFundsAsync(cancellationToken);
      if (existingFunds.Any(f => f.IsActive))
      {
        return ServiceResult.Failure("An active pension fund already exists. Delete it before creating a new one.");
      }

      PensionFund fund = new()
      {
        Name = dto.Name,
        Description = dto.Description,
        TaxCode = dto.TaxCode,
        MonthlySalary = 0,
        ContributionPercentage = 0,
        ContributionAmount = 0,
        PensionOptionId = null,
        IsActive = true
      };

      await fundRepo.AddPensionFundAsync(fund, cancellationToken);
      await fundRepo.SaveChangesAsync(cancellationToken);

      return ServiceResult.Success("Pension fund created successfully.");
    }


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
        return ServiceResult.Failure("Employee or Pension Option not found.");

      if (employee.EmploymentStatus != EmploymentStatus.Permanent)
        return ServiceResult.Failure("Only permanent employees may select a pension option.");

      // Fetch the active pension fund
      PensionFund? activeFund = (await fundRepo.GetPensionFundsAsync(cancellationToken))
                                  .FirstOrDefault(f => f.IsActive);

      if (activeFund == null)
      {
        return ServiceResult.Failure("No active pension fund available.");
      }

      // Check if employee already has a fund record
      PensionFund? fundRecord = await fundRepo.GetPensionFundByEmployeeIdAsync(employee.EmployeeId, cancellationToken);

      if (fundRecord == null)
      {
        // Create new record linked to active fund
        fundRecord = new PensionFund
        {
          EmployeeId = employee.EmployeeId,
          EmployeeName = employee.Name,
          PensionOptionId = option.PensionOptionId,
          MonthlySalary = employee.MonthlySalary,
          ContributionPercentage = option.ContributionPercentage,
          ContributionAmount = employee.MonthlySalary * (option.ContributionPercentage / 100),
          TaxCode = activeFund.TaxCode,
          Name = activeFund.Name,             //  use active fund name
          Description = activeFund.Description // use active fund description
        };

        await fundRepo.AddPensionFundAsync(fundRecord, cancellationToken);
      }
      else
      {
        // Update existing record
        fundRecord.PensionOptionId = option.PensionOptionId;
        fundRecord.ContributionPercentage = option.ContributionPercentage;
        fundRecord.ContributionAmount = employee.MonthlySalary * (option.ContributionPercentage / 100);
        fundRecord.TaxCode = activeFund.TaxCode;
        fundRecord.Name = activeFund.Name;
        fundRecord.Description = activeFund.Description;

        await fundRepo.UpdatePensionFundAsync(fundRecord, cancellationToken);
      }

      // Save changes
      await fundRepo.SaveChangesAsync(cancellationToken);

      // Update employee record
      employee.PensionOptionId = option.PensionOptionId;

      return ServiceResult.Success("Pension option selected and employee updated.");
    }



    public async Task<ServiceResult> DeleteAllOptionsAsync(CancellationToken cancellationToken)
    {
      // Clear PensionOptionId for all employees
      IEnumerable<Employee> employees = await employeeRepo.GetEmployeesAsync(cancellationToken);
      foreach (Employee emp in employees)
      {
        emp.PensionOptionId = null;
      }
      await employeeRepo.SaveChangesAsync(cancellationToken);

      // Clear PensionOptionId for all funds AND mark them inactive
      IEnumerable<PensionFund> funds = await fundRepo.GetPensionFundsAsync(cancellationToken);
      foreach (PensionFund fund in funds)
      {
        fund.PensionOptionId = null;
        fund.IsActive = false;   // mark inactive here
      }
      await fundRepo.SaveChangesAsync(cancellationToken);

      // Delete all options
      await optionRepo.DeleteAllPensionOptionsAsync(cancellationToken);

      return ServiceResult.Success("All pension options deleted and pension funds marked inactive. Employees preserved.");
    }

  }
}

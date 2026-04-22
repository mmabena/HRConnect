namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public interface IPensionFundService
  {
    // Pension Funds
    Task<IEnumerable<PensionFund>> GetPensionFundsAsync();

    Task<PensionFund?> GetPensionFundByIdAsync(int id);

    Task<ServiceResult> AddPensionFundAsync(PensionFund fund);

    Task<ServiceResult> UpdatePensionFundAsync(PensionFund fund);

    // Pension Options
    Task<IEnumerable<PensionOption>> GetPensionOptionsAsync();

    Task<PensionOption?> GetPensionOptionByIdAsync(int id);

    Task<ServiceResult> AddPensionOptionAsync(PensionOption pensionOption);

    Task<ServiceResult> UpdatePensionOptionAsync(PensionOption pensionOption);

    // Pension Deduction
    decimal CalculatePensionDeduction(decimal monthlySalary, PensionOption pensionOption);

    // Employee Selection
    Task<ServiceResult> RecordEmployeePensionSelectionAsync(string employeeId, int PensionOptionId);
  }
}
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
    Task<ServiceResult> AddPensionOptionAsync(PensionOption pensionoption);
    Task<ServiceResult> UpdatePensionOptionAsync(PensionOption pensionoption);

    // Pension Deduction
    decimal CalculatePensionDeduction(decimal monthlySalary, PensionOption pensionoption);

    // Employee selects Pension Option
    Task<ServiceResult> RecordEmployeePensionSelectionAsync(string employeeId, int pensionOptionId);
  }
}

namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public interface IPensionFundService
  {
    // Pension Funds
    Task<IEnumerable<PensionFund>> GetPensionFundsAsync(CancellationToken cancellationToken);

    Task<PensionFund?> GetPensionFundByIdAsync(int id, CancellationToken cancellationToken);

    Task<ServiceResult> AddPensionFundAsync(PensionFund fund, CancellationToken cancellationToken);

    Task<ServiceResult> UpdatePensionFundAsync(PensionFund fund, CancellationToken cancellationToken);

    // Pension Options
    Task<IEnumerable<PensionOption>> GetPensionOptionsAsync(CancellationToken cancellationToken);

    Task<PensionOption?> GetPensionOptionByIdAsync(int id, CancellationToken cancellationToken);

    Task<ServiceResult> AddPensionOptionAsync(PensionOption pensionOption, CancellationToken cancellationToken);

    Task<ServiceResult> UpdatePensionOptionAsync(PensionOption pensionOption, CancellationToken cancellationToken);

    // Pension Deduction
    decimal CalculatePensionDeduction(decimal monthlySalary, PensionOption pensionOption);

    // Employee Selection
    Task<ServiceResult> RecordEmployeePensionSelectionAsync(string employeeId, int pensionOptionId, CancellationToken cancellationToken);
  }
}

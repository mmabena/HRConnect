namespace HRConnect.Api.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Models.CompanyContributions;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.CompanyContribution;
  public interface ICompanyContributionAllocationService
  {
    Task<List<PayrollRecord>> AllocateAsync(int payrollRunId);
  }
}
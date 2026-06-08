namespace HRConnect.Api.Interfaces
{
  using System.Collections.Generic;
  using HRConnect.Api.Models.CompanyContributions;
  using System.Threading.Tasks;

  public interface IEmployeeCompanyContributionService
  {
    Task<List<EmployeeCompanyContribution>> GetByPayRunIdAsync(int payRunId);
  }
}
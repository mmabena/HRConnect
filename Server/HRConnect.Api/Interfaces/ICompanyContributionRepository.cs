namespace HRConnect.Api.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Models.CompanyContributions;
  using HRConnect.Api.DTOs.CompanyContribution;
  public interface ICompanyContributionRepository
  {
    Task<List<CompanyContribution>> GetAllCompanyContributionAsync();
    Task<CompanyContribution?> GetCompanyContributionByIdAsync(int companyContributionId);
    Task<CompanyContribution?> CreateCompanyContributionAsync(CompanyContribution companyContributionModel);
    Task<CompanyContribution?> UpdateCompanyContributionAsync(CompanyContribution companyContributionModel);
    Task DeleteCompanyContributionAsync(int companyContributionId);

  }
}
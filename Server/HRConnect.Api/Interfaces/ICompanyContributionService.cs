namespace HRConnect.Api.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs.CompanyContribution;
  using HRConnect.Api.Models.CompanyContributions;
  public interface ICompanyContributionService
  {
    Task<List<CompanyContributionDto>> GetAllCompanyContributionAsync();
    Task<CompanyContributionDto?> GetCompanyContributionByIdAsync(int id);
    Task<CompanyContributionDto> CreateCompanyContributionAsync(CompanyContribution companyContributionModel);
    Task<CompanyContributionDto> UpdateCompanyContributionAsync(CompanyContribution companyContributionModel);
    Task<bool> FindAllocatedContribution(int payrollRunId);
    Task DeleteAsync(int id);
  }
}
namespace HRConnect.Api.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Models.CompanyContributions;
    using HRConnect.Api.DTOs.CompanyContribution;

    public interface ICompanyContributionService
    {
        Task<List<CompanyContributionDto>> GetAllCompanyContributionAsync();
        Task<CompanyContributionDto?> GetCompanyContributionByIdAsync(int id);
        Task<CompanyContributionDto> CreateCompanyContributionAsync(CompanyContribution companyContributionModel);
        Task<CompanyContributionDto> UpdateCompanyContributionAsync(CompanyContribution companyContributionModel);
        Task DeleteAsync(int id);
    }
    
}
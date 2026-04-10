namespace HRConnect.Api.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Models.CompanyContributions;
    using HRConnect.Api.DTOs.CompanyContribution;
    public interface ICompanyContributionRepository
    {
        Task<List<CompanyContribution>> GetAllAsync();
        Task<CompanyContribution?> GetByIdAsync(int id);
        Task<CompanyContribution?> CreateCompanyContributionAsync(CompanyContribution companyContributionModel);
        Task<CompanyContribution?> UpdateCompanyContributionAsync(CompanyContribution companyContributionModel);
        Task DeleteAsync(int id);
        
    }
}
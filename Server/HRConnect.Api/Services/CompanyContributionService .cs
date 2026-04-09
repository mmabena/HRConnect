namespace HRConnect.Api.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models.CompanyContributions;
    using HRConnect.Api.DTOs.CompanyContribution;

    public class CompanyContributionService : ICompanyContributionService
    {
        private readonly ICompanyContributionRepository _repo;

        public CompanyContributionService(ICompanyContributionRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<CompanyContributionDto>> GetAllCompanyContributionAsync()
        {
            var list = await _repo.GetAllAsync();

            var result = new List<CompanyContributionDto>();

            foreach (var item in list)
            {
                result.Add(Map(item));
            }

            return result;
        }

        public async Task<CompanyContributionDto?> GetCompanyContributionByIdAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return null;

            return Map(item);
        }

        public async Task<CompanyContributionDto> CreateCompanyContributionAsync(CompanyContribution companyContributionModel)
        {
            var created = await _repo.CreateCompanyContributionAsync(companyContributionModel);
            return Map(created!);
        }

        public async Task<CompanyContributionDto> UpdateCompanyContributionAsync(CompanyContribution companyContributionModel)
        {
            var updated = await _repo.UpdateCompanyContributionAsync(companyContributionModel);
            return Map(updated!);
        }

        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }

        private static CompanyContributionDto Map(CompanyContribution c)
        {
            return new CompanyContributionDto
            {
                CompanyContributionId = c.CompanyContributionId,
                Code = c.Code,
                ShortDescription = c.ShortDescription,
                LongDescription = c.LongDescription,
                TaxCode = c.TaxCode,
                Percentage = c.Percentage,
                IsActive = c.IsActive
            };
        }
    }
}
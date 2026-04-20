namespace HRConnect.Api.Mappers
{
    using HRConnect.Api.DTOs.CompanyContribution;
    using HRConnect.Api.Models;
    using HRConnect.Api.Models.CompanyContributions;

    public static class CompanyContributionMapper
    {
        public static CompanyContributionDto ToCompanyContributionDto(this CompanyContribution companyContributionModel)
        {
            return new CompanyContributionDto
            {
                CompanyContributionId = companyContributionModel.CompanyContributionId,
                Code = companyContributionModel.Code,
                ShortDescription = companyContributionModel.ShortDescription,
                LongDescription = companyContributionModel.LongDescription,
                TaxCode = companyContributionModel.TaxCode,
                Percentage = companyContributionModel.Percentage,
                IsActive = companyContributionModel.IsActive
            };
        }

        public static CompanyContribution ToCompanyContributionFromCreateDTO(this CreateCompanyContributionDto createCompanyContributionDto)
        {
            return new CompanyContribution
            {
                Code = createCompanyContributionDto.Code,
                ShortDescription = createCompanyContributionDto.ShortDescription,
                LongDescription = createCompanyContributionDto.LongDescription,
                TaxCode = createCompanyContributionDto.TaxCode,
                Percentage = createCompanyContributionDto.Percentage,
                IsActive = true // New contributions are active by default
            };
        }
    }
}
namespace HRConnect.Api.Interfaces
{
    using HRConnect.Api.Models.CompanyContributions;
    public interface IEmployeeCompanyContributionRepository
    {
        Task AddRangeAsync(List<EmployeeCompanyContribution> records);
    }
}
namespace HRConnect.Api.Repository
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models.CompanyContributions;

    public class EmployeeCompanyContributionRepository : IEmployeeCompanyContributionRepository
    {
        private readonly ApplicationDBContext _context;

        public EmployeeCompanyContributionRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(List<EmployeeCompanyContribution> records)
        {
            await _context.EmployeeCompanyContributions.AddRangeAsync(records);
            await _context.SaveChangesAsync();
        }
    }
}
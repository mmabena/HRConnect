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
        /// <summary>
        /// Saves a batch of employee contribution records to the database.
        /// </summary>
        /// <param name="records">The list of employee contribution records to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddRangeAsync(List<EmployeeCompanyContribution> records)
        {
            await _context.EmployeeCompanyContributions.AddRangeAsync(records);
            await _context.SaveChangesAsync();
        }
    }
}
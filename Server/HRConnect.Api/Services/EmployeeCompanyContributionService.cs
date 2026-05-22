namespace HRConnect.Api.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models.CompanyContributions;
    using Microsoft.EntityFrameworkCore;

    public class EmployeeCompanyContributionService : IEmployeeCompanyContributionService
    {
        private readonly ApplicationDBContext _context;

        public EmployeeCompanyContributionService(ApplicationDBContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Retrieves all employee company contribution records for a specific payroll run.
        /// </summary>
        /// <param name="payRunId">The payroll run identifier.</param>
        /// <returns>List of employee company contribution records.</returns>
        public async Task<List<EmployeeCompanyContribution>> GetByPayRunIdAsync(int payRunId)
        {
            return await _context.EmployeeCompanyContributions
                .Where(x => x.PayrollRunId == payRunId)
                .ToListAsync();
        }
    }
}
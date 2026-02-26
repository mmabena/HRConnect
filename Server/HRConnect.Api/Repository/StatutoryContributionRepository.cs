namespace HRConnect.Api.Repository
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Models;
    using HRConnect.Api.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class StatutoryContributionRepository : IStatutoryContributionRepository
    {
        private readonly ApplicationDBContext _context;
        public StatutoryContributionRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<StatutoryContribution> AddDeductionsAsync(StatutoryContribution statutoryContribution)
        {
            await _context.StatutoryContributions.AddAsync(statutoryContribution);
            await _context.SaveChangesAsync();
            return statutoryContribution;
        }
        public async Task<IEnumerable<StatutoryContribution>> GetAllDeductionsAsync()
        {
            return await _context.StatutoryContributions.ToListAsync();
        }

        public async Task<StatutoryContribution?> GetDeductionsByEmployeeIdAsync(string employeeId)
        {
            return await _context.StatutoryContributions.FirstOrDefaultAsync(p => p.EmployeeId == employeeId);
        }

    }
}
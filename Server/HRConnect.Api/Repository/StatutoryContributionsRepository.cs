namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces;
  using Microsoft.EntityFrameworkCore;

  public class StatutoryContributionsRepository : IStatutoryContributionRepository
  {
    private readonly ApplicationDBContext _context;
    public StatutoryContributionsRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    public async Task<StatutoryContribution> AddDeductionsAsync(StatutoryContribution payrollDeductions)
    {
      await _context.StatutoryContributions.AddAsync(payrollDeductions);
      await _context.SaveChangesAsync();
      return payrollDeductions;
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
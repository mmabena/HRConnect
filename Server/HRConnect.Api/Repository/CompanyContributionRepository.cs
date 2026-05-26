namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.CompanyContributions;
  using Microsoft.EntityFrameworkCore;

  public class CompanyContributionRepository : ICompanyContributionRepository
  {
    private readonly ApplicationDBContext _context;

    public CompanyContributionRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    public async Task<bool> FindAllocatedContribution(int payrollRunId)
    {
      return await _context.EmployeeCompanyContributions
        .AnyAsync(e => e.PayrollRunId == payrollRunId);
    }

    public async Task<List<CompanyContribution>> GetAllAsync()
    {
      return await _context.CompanyContributions.ToListAsync();
    }

    public async Task<CompanyContribution?> GetByIdAsync(int id)
    {
      return await _context.CompanyContributions.FindAsync(id);
    }

    public async Task<CompanyContribution?> CreateCompanyContributionAsync(CompanyContribution companyContributionModel)
    {
      await _context.CompanyContributions.AddAsync(companyContributionModel);
      await _context.SaveChangesAsync();
      return companyContributionModel;
    }

    public async Task<CompanyContribution?> UpdateCompanyContributionAsync(CompanyContribution companyContributionModel)
    {
      _context.CompanyContributions.Update(companyContributionModel);
      await _context.SaveChangesAsync();
      return companyContributionModel;
    }

    public async Task DeleteAsync(int id)
    {
      var entity = await GetByIdAsync(id);
      if (entity != null)
      {
        _context.CompanyContributions.Remove(entity);
        await _context.SaveChangesAsync();
      }
    }
  }
}
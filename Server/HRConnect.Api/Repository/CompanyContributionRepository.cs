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


    /// <summary>
    /// Retrieves all company contribution records.
    /// </summary>
    /// <returns>A list of all CompanyContribution entities.</returns>
    public async Task<List<CompanyContribution>> GetAllCompanyContributionAsync()
    {
      return await _context.CompanyContributions.ToListAsync();
    }


    /// <summary>
    /// Retrieves a company contribution by its unique identifier.
    /// </summary>
    /// <param name="companyContributionId">The contribution ID.</param>
    /// <returns>The matching CompanyContribution entity, or null if not found.</returns>
    public async Task<CompanyContribution?> GetCompanyContributionByIdAsync(int companyContributionId)
    {
      return await _context.CompanyContributions.FindAsync(companyContributionId);
    }
    /// <summary>
    /// Creates a new company contribution record.
    /// </summary>
    /// <param name="companyContributionModel">The CompanyContribution entity to create.</param>
    /// <returns>The created CompanyContribution entity.</returns>
    public async Task<CompanyContribution?> CreateCompanyContributionAsync(CompanyContribution companyContributionModel)
    {
      await _context.CompanyContributions.AddAsync(companyContributionModel);
      await _context.SaveChangesAsync();
      return companyContributionModel;
    }
    /// <summary>
    /// Updates an existing company contribution record.
    /// </summary>
    /// <param name="companyContributionModel">The updated CompanyContribution entity.</param>
    /// <returns>The updated CompanyContribution entity.</returns>
    public async Task<CompanyContribution?> UpdateCompanyContributionAsync(CompanyContribution companyContributionModel)
    {
      _context.CompanyContributions.Update(companyContributionModel);
      await _context.SaveChangesAsync();
      return companyContributionModel;
    }
    /// <summary>
    /// Deletes a company contribution by its ID if it exists.
    /// </summary>
    /// <param name="companyContributionId">The ID of the contribution to delete.</param>
    public async Task DeleteCompanyContributionAsync(int companyContributionId)
    {
      var entity = await GetCompanyContributionByIdAsync(companyContributionId);
      if (entity != null)
      {
        _context.CompanyContributions.Remove(entity);
        await _context.SaveChangesAsync();
      }
    }
  }
}
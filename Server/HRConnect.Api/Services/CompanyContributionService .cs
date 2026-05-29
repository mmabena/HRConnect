namespace HRConnect.Api.Services
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs.CompanyContribution;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Models.CompanyContributions;

  public class CompanyContributionService : ICompanyContributionService
  {
    private readonly ICompanyContributionRepository _companyContributionRepo;

    public CompanyContributionService(ICompanyContributionRepository companyContributionRepo)
    {
      _companyContributionRepo = companyContributionRepo;
    }
    /// <summary>
    /// Retrieves all company contribution definitions.
    /// </summary>
    /// <returns>List of company contribution definitions.</returns>

    public async Task<List<CompanyContributionDto>> GetAllCompanyContributionAsync()
    {
      var list = await _companyContributionRepo.GetAllCompanyContributionAsync();

      var companyContributions = new List<CompanyContributionDto>();

      foreach (var contributions in list)
      {
        companyContributions.Add(contributions.ToCompanyContributionDto());
      }

      return companyContributions;
    }
    /// <summary>
    /// Retrieves a single company contribution by ID.
    /// </summary>
    /// <returns>A company contribution definition or null if not found.</returns>
    public async Task<CompanyContributionDto?> GetCompanyContributionByIdAsync(int companyContributionId)
    {
      var contributions = await _companyContributionRepo.GetCompanyContributionByIdAsync(companyContributionId);
      if (contributions == null) return null;

      return contributions.ToCompanyContributionDto();
    }
    /// <summary>
    /// Creates a new company contribution rule.
    /// </summary>
    /// <param name="companyContributionModel">The company contribution model to create.</param>
    /// <returns>The created company contribution definition.</returns>
    public async Task<CompanyContributionDto> CreateCompanyContributionAsync(CompanyContribution companyContributionModel)
    {
      var createdContribution = await _companyContributionRepo.CreateCompanyContributionAsync(companyContributionModel);
      return createdContribution.ToCompanyContributionDto();
    }

    /// <summary>
    /// Updates an existing company contribution rule.
    /// </summary>
    /// <param name="companyContributionModel">The company contribution model to update.</param>
    /// <returns>The updated company contribution definition.</returns>
    public async Task<CompanyContributionDto> UpdateCompanyContributionAsync(CompanyContribution companyContributionModel)
    {
      var updatedContribution = await _companyContributionRepo.UpdateCompanyContributionAsync(companyContributionModel);
      return updatedContribution.ToCompanyContributionDto();
    }

    /// <summary>
    /// Deletes a company contribution rule.
    /// </summary>
    /// <param name="companyContributionId">The ID of the company contribution to delete.</param>
    public async Task DeleteCompanyContributionAsync(int companyContributionId)
    {
      await _companyContributionRepo.DeleteCompanyContributionAsync(companyContributionId);
    }

     /// <summary>
    /// Find any allocated company contribution for the given payroll run
    /// </summary>
    /// <param name="payrollRunId">Id for given payroll run</param>
    /// <returns></returns>
    public async Task<bool> FindAllocatedContribution(int payrollRunId)
    {
      return await _companyContributionRepo.FindAllocatedContribution(payrollRunId);
    }

  }
}

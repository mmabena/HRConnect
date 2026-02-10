namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  using HRConnect.Api.Repository.Models;
  using Microsoft.EntityFrameworkCore.Infrastructure;

  public class MedicalOptionRepository: IMedicalOptionRepository
  {
    private readonly ApplicationDBContext _context;
    
    public MedicalOptionRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    
    public async Task<List<MedicalOptionFlatRow>> GetGroupedMedicalOptionsAsync()
    {
      //TODO: Implement method
      
      //TODO: Extract the logic to a service
      
      // The query
      return await
      (
        from parent in _context.MedicalOptionCategories
        join child in _context.MedicalOptionCategories
          on parent.MedicalOptionCategoryId equals child.MedicalOptionParentCategoryId
        join option in _context.MedicalOptions
          on child.MedicalOptionCategoryId equals option.MedicalOptionCategoryId
        select new MedicalOptionFlatRow
        (
          parent.MedicalOptionCategoryId,
          child.MedicalOptionParentCategoryId,
          parent.MedicalOptionCategoryName,
          parent.MonthlyRiskContributionPrincipal, 
          parent.MonthlyRiskContributionAdult,
          parent.MonthlyRiskContributionChild,
          parent.MonthlyRiskContributionChild2,
          parent.MonthlyMsaContributionPrincipal,
          parent.MonthlyMsaContributionAdult,
          parent.MonthlyMsaContributionChild,
          parent.TotalMonthlyContributionsPrincipal,
          parent.TotalMonthlyContributionsAdult,
          parent.TotalMonthlyContributionsChild,
          parent.TotalMonthlyContributionsChild2,
          //fields from policy option table
          option.MedicalOptionId,
          option.MedicalOptionName,
          option.MedicalOptionCategoryId,
          option.SalaryBracketMin,
          option.SalaryBracketMax
        )
      ).ToListAsync();
    }
  }
}
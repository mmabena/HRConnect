namespace HRConnect.Api.Services
{
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Interfaces;

  public class MedicalOptionService:IMedicalOptionService
  {
    // TODO: Implement methods
    private readonly IMedicalOptionRepository _medicalOptionRepository;

    public MedicalOptionService(IMedicalOptionRepository medicalOptionRepository)
    {
      _medicalOptionRepository = medicalOptionRepository;
    }
    
    public async Task<List<MedicalOptionCategoryGroupDto>> GetGroupedMedicalOptionsAsync()
    {
      //Get Rows from repository method
      var rows = await _medicalOptionRepository.GetGroupedMedicalOptionsAsync();
      
      //Group Options by Parent option name
      /*return rows
        .GroupBy(r => new
          { r.BaseMedicalOptionParentCategoryId, r.BaseMedicalPolicyOptionCategoryName })
        .Select(g => new MedicalOptionCategoryGroupDto
        {
          MedicalOptionGroupName = g.Key.BaseMedicalPolicyOptionCategoryName,
          Options = g.Select(r => new MedicalOptionDto
          {
            MedicalOptionId = r.BaseMedicalOptionId,
            MedicalOptionName = r.BaseMedicalOptionName,
            MedicalOptionCategoryId = r.BaseMedicalOptionCategoryId,
            SalaryBracketMin = r.BaseSalaryBracketMin,
            SalaryBracketMax = r.BaseSalaryBracketMax
          }).ToList()
        }).ToList();
        */
      return rows.ToMedicalOptionCategoryGroupDto();
    }
  }  
}
namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Models;

  public interface IMedicalOptionService
  {
    Task<List<MedicalOptionCategoryDto>> GetGroupedMedicalOptionsAsync();
    Task<MedicalOption?> UpdateSalaryBracketAsync(int id, 
      UpdateMedicalOptionSalaryBracketRequestDto requestDto);
    Task<MedicalOption?> GetMedicalOptionByIdAsync(int id);
  }  
}

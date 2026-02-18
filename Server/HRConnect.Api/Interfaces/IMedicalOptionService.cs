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
    Task<Boolean> MedicalOptionCategoryExistsAsync(int categoryId);
    Task<Boolean> MedicalOptionExistsAsync(int optionId);
    Task<List<MedicalOption>> GetAllOptionsUnderCategoryAsync(int categoryId);

    Task<IReadOnlyList<MedicalOption?>> BulkUpdateByCategoryIdAsync(int categoryId,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto);
  }  
}

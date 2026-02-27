namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Models;

  public interface IMedicalOptionService
  {
    Task<List<MedicalOptionCategoryDto>> GetGroupedMedicalOptionsAsync();
    Task<MedicalOptionDto?> GetMedicalOptionByIdAsync(int id);
    Task<MedicalOptionDto?> GetMedicalOptionCategoryByIdAsync(int categoryId);
    Task<Boolean> MedicalOptionCategoryExistsAsync(int categoryId);
    Task<Boolean> MedicalOptionExistsAsync(int optionId);
    Task<List<MedicalOptionDto?>> GetAllOptionsUnderCategoryAsync(int categoryId);
    Task<Boolean> MedicalOptionExistsWithinCategoryAsync(int categoryId, int optionId);
    public Task<IReadOnlyList<MedicalOptionDto>> BulkUpdateMedicalOptionsByCategoryAsync(
      int categoryId, IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto);
  }  
}

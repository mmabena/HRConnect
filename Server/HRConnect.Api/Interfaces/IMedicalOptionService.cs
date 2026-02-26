namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Models;

  public interface IMedicalOptionService
  {
    Task<List<MedicalOptionCategoryDto>> GetGroupedMedicalOptionsAsync();
    Task<MedicalOption?> GetMedicalOptionByIdAsync(int id);
    Task<MedicalOption?> GetMedicalOptionCategoryByIdAsync(int categoryId);
    Task<Boolean> MedicalOptionCategoryExistsAsync(int categoryId);
    Task<Boolean> MedicalOptionExistsAsync(int optionId);
    Task<List<MedicalOption>> GetAllOptionsUnderCategoryAsync(int categoryId);
    Task<Boolean> MedicalOptionExistsWithinCategoryAsync(int categoryId, int optionId);
    public Task<IReadOnlyList<MedicalOptionDto>> BulkUpdateMedicalOptionsByCategoryAsync(
      int categoryId, IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto);
  }  
}

namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Models;


  public interface IMedicalOptionRepository
  {
    //TODO: Document methods
    //1. GetGrouped MedicalOptions
    //2. GetMedicalOptionById
    //3. GetMedicalOptionByCategory
    //4. GetMedicalOptionBySalaryBracketGetAllMedicalOptionsUnderCategoryVariantAsync
    //5. GetMedicalOptionByEmployee
    //6. UpdateSalaryBracket (Put)
    
    Task<List<IGrouping<int, MedicalOption>>> GetGroupedMedicalOptionsAsync();
    Task<MedicalOptionDto?> GetMedicalOptionByIdAsync(int id);
    Task<MedicalOptionDto?> GetMedicalOptionCategoryByIdAsync(int id);
    Task<List<MedicalOptionDto>> GetMedicalOptionsByIdsAsync(List<int> ids);
    Task<MedicalOptionCategory?> GetCategoryByIdAsync(int id);
    Task<List<MedicalOptionDto?>> GetAllMedicalOptionsUnderCategoryVariantAsync(string optionName);
    //helper functions within Repository
    Task<Boolean> MedicalOptionCategoryExistsAsync(int categoryId);
    Task<Boolean> MedicalOptionExistsAsync(int optionId);
    
    Task<List<MedicalOptionDto>> GetAllOptionsUnderCategoryAsync(int categoryId);

    Task<Boolean> MedicalOptionExistsWithinCategoryAsync(int categoryId, int optionId);
    
    Task<IReadOnlyList<MedicalOptionDto>> BulkUpdateByCategoryIdAsync(int categoryId,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto);
    
    //Task<List<MedicalOption?>> GetMedicalOptionsByCategoryAsync(int categoryId);
    //Task<List<MedicalOption>> GetMedicalOptionBySalaryBracketAsync(
    //decimal salaryBracketMin, decimal salaryBracketMax);
    //Task<List<MedicalOption>> GetMedicalOptionByEmployeeAsync(int employeeId);

  }
}
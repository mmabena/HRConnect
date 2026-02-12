namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Models;


  public interface IMedicalOptionRepository
  {
    //TODO: Document methods
    //1. GetGroupedMedicalOptions
    //2. GetMedicalOptionById
    //3. GetMedicalOptionByCategory
    //4. GetMedicalOptionBySalaryBracket
    //5. GetMedicalOptionByEmployee
    //6. UpdateSalaryBracket (Put)
    
    Task<List<IGrouping<int, MedicalOption>>> GetGroupedMedicalOptionsAsync();
    Task<MedicalOption?> GetMedicalOptionByIdAsync(int id);
    Task<MedicalOption?> UpdateSalaryBracketAsync(int id, 
      UpdateMedicalOptionSalaryBracketRequestDto requestDto);
    Task<MedicalOption?> GetMedicalOptionCategoryByIdAsync(int id);
    Task<List<MedicalOption?>> GetAllMedicalOptionsUnderCategoryVarientAsync(string optionName);
    //Task<List<MedicalOption?>> GetMedicalOptionsByCategoryAsync(int categoryId);
    //Task<List<MedicalOption>> GetMedicalOptionBySalaryBracketAsync(
    //decimal salaryBracketMin, decimal salaryBracketMax);
    //Task<List<MedicalOption>> GetMedicalOptionByEmployeeAsync(int employeeId);

  }
}
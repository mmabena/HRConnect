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
    
  }
}
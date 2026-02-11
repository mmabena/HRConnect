namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Models;

  public interface IMedicalOptionService
  {
    Task<List<MedicalOptionCategoryDto>> GetGroupedMedicalOptionsAsync();
  }  
}

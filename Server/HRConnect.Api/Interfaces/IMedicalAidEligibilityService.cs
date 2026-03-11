namespace HRConnect.Api.Interfaces
{
  using DTOs.MedicalOption;

  public interface IMedicalAidEligibilityService
  {
    Task<IReadOnlyList<MedicalOptionCategoryDto>> GetEligibleMedicalOptionsForEmployee(string employeeId);
    

  }
}
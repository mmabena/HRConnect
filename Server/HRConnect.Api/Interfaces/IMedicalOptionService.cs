namespace HRConnect.Api.Interfaces;

using DTOs.MedicalOption;

public interface IMedicalOptionService
{
  Task<List<MedicalOptionCategoryGroupDto>> GetGroupedMedicalOptionsAsync();
}
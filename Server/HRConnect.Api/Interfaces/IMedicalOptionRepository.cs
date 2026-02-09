namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  public interface IMedicalOptionRepository
  {
    //TODO: Document methods
    Task<List<MedicalOption>> GetAllMedicalOptionsGroupedByCategoryAsync();
  }
}
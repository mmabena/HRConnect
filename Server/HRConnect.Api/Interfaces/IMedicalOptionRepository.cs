namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using Repository.Models;

  public interface IMedicalOptionRepository
  {
    //TODO: Document methods
    /// <summary>
    /// Gets flat rows: base categories joined with their policy options
    /// Returns all fields from both tables
    /// </summary>
    Task<List<MedicalOptionFlatRow>> GetGroupedMedicalOptionsAsync();
    
  }
}
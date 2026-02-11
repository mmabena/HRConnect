namespace HRConnect.Api.DTOs.MedicalOption
{
  /// <summary>
  /// The Medical Option Category Data Transfer Object represent a singles instance
  /// of a medical option category to be sent to the client, where it will be used
  /// to display the medical option category and options details to the client
  /// </summary>
  
  public class MedicalOptionCategoryDto
  {
    public int MedicalOptionCategoryId { get; set; }
    public string MedicalOptionCategoryName { get; set; } = string.Empty;
    public ICollection<MedicalOptionDto> MedicalOptions { get; set; } = new List<MedicalOptionDto>();
  }  
}

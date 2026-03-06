namespace HRConnect.Api.DTOs.MedicalOption
{
  public class UpdateMedicalOptionCategoryDto
  {
    public int MedicalOptionCategoryId { get; set; }
    public string MedicalOptionCategoryName { get; set; } = string.Empty;
  }
}
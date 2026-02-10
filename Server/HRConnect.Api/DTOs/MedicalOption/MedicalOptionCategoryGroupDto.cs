namespace HRConnect.Api.DTOs.MedicalOption;

public class MedicalOptionCategoryGroupDto
{
  public int MedicalOptionParentCategoryId { get; set; }
  public string MedicalOptionGroupName { get; set; } = string.Empty;
  
  public List<MedicalOptionDto> Options { get; set; } = new();
}
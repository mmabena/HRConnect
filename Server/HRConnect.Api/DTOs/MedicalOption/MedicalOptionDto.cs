namespace HRConnect.Api.DTOs.MedicalOption
{
  using HRConnect.Api.Models;

  public class MedicalOptionDto
  {
    public int MedicalOptionId { get; set; }
    public string MedicalOptionName { get; set; } = string.Empty;
    public int MedicalOptionCategoryId { get; set; }
    public decimal? SalaryBracketMin { get; set; }
    public decimal? SalaryBracketMax { get; set; }
    // Navigation property to MedicalOptionCategory 1:1 relationship
    public MedicalOptionCategory Category { get; set; } = null!;
  }
}
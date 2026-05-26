namespace HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;

public class UpdateMedicalAidDeductionRequestDto
{
  public int MedicalOptionId { get; set; }
  public string OptionName { get; set; } = string.Empty;
  public int MedicalCategoryId { get; set; }
  public string OptionCategory { get; set; } = string.Empty;
  public int PrincipalCount { get; set; }
  public int AdultCount { get; set; }
  public int ChildrenCount { get; set; }
}
namespace HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;

public class UpdateMedicalAidDeductionRequestDto
{
  //FK
  public int MedicalOptionId { get; set; }
  public string OptionName { get; set; }
  //FK
  public int MedicalCategoryId { get; set; }
  public string OptionCategory { get; set; }

  // Number of Deps
  public int PrincipalCount { get; set; }
  public int AdultCount { get; set; }
  public int ChildrenCount { get; set; }
  
  public DateTime UpdatedDate { get; set; } = DateTime.Now.ToLocalTime();
}
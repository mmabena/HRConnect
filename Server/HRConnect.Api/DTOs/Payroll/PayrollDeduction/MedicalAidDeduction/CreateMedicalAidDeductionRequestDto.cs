namespace HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;

/// <summary>
/// Request DTO for creating a new medical aid deduction.
/// Contains the selected medical option details from the eligibility check.
/// </summary>
public class CreateMedicalAidDeductionRequestDto
{
  public int MedicalOptionId { get; set; }
  // Number of Deps
  public int PrincipalCount { get; set; }
  public int AdultCount { get; set; }
  public int ChildrenCount { get; set; }
}

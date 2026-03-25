namespace HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction
{
  public class TerminateMedicalAidDeductionRequestDto
  {
    public int MedicalOptionId { get; set; }
    public string TerminationReason { get; set; }
  }
}
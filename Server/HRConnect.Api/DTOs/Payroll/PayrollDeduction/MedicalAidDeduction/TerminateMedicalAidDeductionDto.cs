namespace HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;

public class TerminateMedicalAidDeductionDto
{
  public DateTime TerminationDate {get; set;} = DateTime.Now.ToLocalTime();
  public string TerminationReason {get; set;}
  public int OptionId {get; set;}
  public int PrincipalCount { get; set; }
  public int AdultCount { get; set; } 
  public int ChildrenCount { get; set; }
  public DateTime UpdatedDate { get; set; } = DateTime.Now.ToLocalTime();
}
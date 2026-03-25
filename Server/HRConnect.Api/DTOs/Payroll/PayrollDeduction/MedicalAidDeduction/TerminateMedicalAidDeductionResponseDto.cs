namespace HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction
{
  public class TerminateMedicalAidDeductionResponseDto
  {
    public int Id { get; set; }
    public string EmployeeId {get; set; }
    public int MedicalOptionId { get; set; }
    public string OptionName { get; set; }
    
    //Before Termination
    public int PreviousPrincipalCount { get; set; }
    public int PreviousAdultCount { get; set; }
    public int PreviousChildrenCount { get; set; }

    public decimal PreviousPrincipalPremium { get; set; }
    public decimal? PreviousSpousePremium { get; set; }
    public decimal? PreviousChildrenPremium { get; set; }
    public decimal PreviousTotalDeductionAmount { get; set; }
    
    //After Termination
    public int PrincipalCount { get; set; }
    public int AdultCount { get; set; }
    public int ChildrenCount { get; set; }
    public decimal PrincipalPremium { get; set; }
    public decimal? SpousePremium { get; set; }
    public decimal? ChildPremium { get; set; }
    public decimal TotalDeductionAmount { get; set; }

    public DateTime TerminationDate { get; set; }
    public string TerminationReason { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime UpdatedDate { get; set; }
  }
}
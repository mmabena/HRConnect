namespace HRConnect.Api.Models.PayrollDeduction
{
  using System.ComponentModel.DataAnnotations.Schema;
  using HRConnect.Api.Models.Payroll;

  public class PensionDeduction : PayrollRecord
  {
    public int PensionDeductionId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateJoinedCompany { get; set; }
    public string IDNumber { get; set; } = string.Empty;
    public string? Passport { get; set; }
    public string TaxNumber { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)")]
    public decimal PensionableSalary { get; set; }
    public int PensionOptionId { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal PendsionCategoryPercentage { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal PensionContribution { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal VoluntaryContribution { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public string PhyscialAddress { get; set; } = string.Empty;
    //public int PayrollRunId { get; set; }
    public DateOnly CreatedDate { get; set; }
    public bool IsActive { get; set; }

    public Employee Employee { get; set; }
    public PensionOption PensionOption { get; set; }
    //public PayrollRun PayrollRun { get; set; }
  }
}


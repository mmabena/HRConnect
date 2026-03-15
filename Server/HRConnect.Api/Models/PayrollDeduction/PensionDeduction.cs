namespace HRConnect.Api.Models.PayrollDeduction
{
  using System.ComponentModel.DataAnnotations.Schema;
  using HRConnect.Api.Models.Payroll;

  public class PensionDeduction : PayrollRecord
  {
    // public int PensionDeductionID { get; set; }
    [Column("EmployeePensionDeductionId")]
    public int EmployeePensionDeductionId { get; set; }
    public int PeriodId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateJoinedCompany { get; set; }
    public string IDNumber { get; set; } = string.Empty;
    public string? Passport { get; set; }
    public string TaxNumber { get; set; } = string.Empty;
    public int PensionOptionId { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal PensionableSalary { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal PendsionCategoryPercentage { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal PensionContribution { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal VoluntaryContribution { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public string PhyscialAddress { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
  }
}


namespace HRConnect.Api.Models.PayrollDeduction
{
  using System.ComponentModel.DataAnnotations.Schema;
  using HRConnect.Api.Models.Payroll;
  using Microsoft.EntityFrameworkCore;

  public class PensionDeduction : PayrollRecord
  {
    [Column("EmployeePensionDeductionId")]
    public int EmployeePensionDeductionId { get; set; }
    public int PeriodId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateJoinedCompany { get; set; }
    public string IdNumber { get; set; } = string.Empty;
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
    [Precision(18, 2)]
    public decimal TotalPensionContribution { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public string PhysicalAddress { get; set; } = string.Empty;
    public DateOnly CreatedDate { get; set; }
    public bool IsActive { get; set; }
  }
}
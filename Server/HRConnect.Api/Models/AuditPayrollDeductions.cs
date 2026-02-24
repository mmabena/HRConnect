namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  using Microsoft.EntityFrameworkCore;

  /// <summary>
  /// Model For storing custom audits based to payroll deductions
  /// </summary>
  public class AuditPayrollDeductions
  {
    [Key]
    public int AuditId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    [Precision(18, 2)]
    public decimal MonthlySalary { get; set; }
    [Precision(18, 2)]
    public decimal ProjectedSalary { get; set; }
    [Precision(5, 2)]
    public decimal UifEmployeeAmount { get; set; }
    [Precision(5, 2)]
    public decimal UifEmployerAmount { get; set; }
    [Precision(18, 2)]
    public decimal EmployerSdlContribution { get; set; }
    public string AuditAction { get; set; } = string.Empty;
    /// <summary>
    /// References name of the table being audited
    /// </summary>
    public string TabelName { get; set; } = string.Empty;
    public DateTime AuditedAt { get; set; } = DateTime.UtcNow;
  }
}
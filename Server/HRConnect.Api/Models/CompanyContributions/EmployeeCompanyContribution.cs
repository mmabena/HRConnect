namespace HRConnect.Api.Models.CompanyContributions
{
  using HRConnect.Api.Models.Payroll;
  using System.ComponentModel.DataAnnotations.Schema;

  public class EmployeeCompanyContribution : PayrollRecord
  {
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;

    public string IdNumber { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;

    public int Age { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; }

    // Contribution values

    public decimal BEEPercentage { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal BEEAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal DeathAmount { get; set; }
    public decimal DeathPercentage { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal DisabilityAmount { get; set; }
    public decimal DisabilityPercentage { get; set; }

  }
}
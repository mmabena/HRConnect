namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  public class PensionFund
  {
    [Key]
    public int PensionFundId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string? EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }
    public string EmployeeName { get; set; } = string.Empty;

    public decimal MonthlySalary { get; set; }

    public decimal ContributionPercentage { get; set; }

    public decimal ContributionAmount { get; set; }

    public int TaxCode { get; set; } = 4001;

    public bool IsActive { get; set; } = true;

    public int? PensionOptionId { get; set; }
    public PensionOption? PensionOptions { get; set; }
  }
}

namespace HRConnect.Api.DTOs
{
  public class PensionFundDto
  {
    public int PensionFundId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public decimal MonthlySalary { get; set; }

    public decimal ContributionPercentage { get; set; }

    public decimal ContributionAmount { get; set; }
    public decimal TaxCode { get; set; } = 4001;

    public int PensionOptionId { get; set; }
    public PensionOptionDto? PensionOption { get; set; }

  }
}

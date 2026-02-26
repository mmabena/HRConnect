namespace HRConnect.Api.DTOs
{
  public class PensionFundDto
  {
    public int PensionFundId { get; set; }
    public string EmployeeId { get; set; }

    public string EmployeeName { get; set; }

    public decimal MonthlySalary { get; set; }

    public decimal ContributionPercentage { get; set; }

    public decimal ContributionAmount { get; set; }
    public decimal TaxCode { get; set; } = 4001;

    public int PensionOptionId { get; set; }
    public PensionOptionDto PensionOption { get; set; }

  }
}

namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;

  public class PensionFund
  {
    [Key]
    public int PensionFundId { get; set; }

    public string EmployeeId { get; set; }

    public string EmployeeName { get; set; }

    public decimal MonthlySalary { get; set; }

    public decimal ContributionPercentage { get; set; }

    public decimal ContributionAmount { get; set; }

    public decimal TaxCode { get; set; } = 4001;

    public int PensionOptionId { get; set; }
    public PensionOption PensionOptions { get; set; }

    public ICollection<Employee> Employees { get; set; }

  }
}

namespace HRConnect.Api.Models.Payroll.Earnings
{
  using System.ComponentModel.DataAnnotations.Schema;

  public class PayrollEarning
  {
    public required string PayrollEarningId { get; set; }
    public required string ShortDescription { get; set; }
    public required string LongDescription { get; set; }
    public bool Taxable { get; set; }
    public int TaxCode { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? HourlyRate { get; set; }
    public bool CanProRata { get; set; }
    public bool IsActive { get; set; }
    public ICollection<EmployeePayrollEarning> EmployeePayrollEarning { get; set; } = [];
  }
}

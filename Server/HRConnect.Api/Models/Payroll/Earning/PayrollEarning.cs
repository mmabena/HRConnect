namespace HRConnect.Api.Models.Payroll.Earning
{
  using System.ComponentModel.DataAnnotations.Schema;

  public class PayrollEarning
  {
    public required string PayrollEarningId { get; set; } = string.Empty;
    public required string ShortDescription { get; set; } = string.Empty;
    public required string LongDescription { get; set; } = string.Empty;
    public bool Taxable { get; set; }
    public int TaxCode { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? TaxPercentage { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? OvertimeHourMultiplier { get; set; }
    public bool CanProRata { get; set; }
    public bool IsOnGoing { get; set; }
    public bool IsActive { get; set; }
  }
}
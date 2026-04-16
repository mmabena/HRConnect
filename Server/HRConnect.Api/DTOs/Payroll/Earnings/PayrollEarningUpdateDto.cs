namespace HRConnect.Api.DTOs.Payroll.Earnings
{
  public class PayrollEarningUpdateDto
  {
    public required string PayrollEarningId { get; set; }
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public bool? Taxable { get; set; }
    public int? TaxCode { get; set; }
    public decimal? TaxPercentage { get; set; }
    public decimal? OvertimeHourMultiplier { get; set; }
    public bool? CanProRata { get; set; }
    public bool? IsOnGoing { get; set; }
    public bool? IsActive { get; set; }
  }
}

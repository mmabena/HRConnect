namespace HRConnect.Api.DTOs.Payroll.Earnings
{
  public class PayrollEarningAddDto
  {
    public string ShortDescription { get; set; } = string.Empty;
    public string LongDescription { get; set; } = string.Empty;
    public bool Taxable { get; set; }
    public int TaxCode { get; set; }
    public decimal? HourlyRate { get; set; }
    public bool CanProRata { get; set; }
    public bool IsActive { get; set; }
  }
}

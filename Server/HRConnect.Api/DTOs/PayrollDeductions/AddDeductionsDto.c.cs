namespace HRConnect.Api.DTOs.PayrollDeductions
{
  public class AddDeductionsDto
  {
    public decimal SdlAmount { get; set; }
    public decimal UifEmployeeAmount { get; set; }
    public decimal UifEmployerAmount { get; set; }
  }
}
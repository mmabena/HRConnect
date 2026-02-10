namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  using Microsoft.EntityFrameworkCore;

  public static class UIFConstants
  {
    public const decimal UIFCap = 17712m;
    public const decimal UIFEmployeeAmount = 0.01m;
    public const decimal UIFEmployerAmount = 0.01m;

  }

  public class PayrollDeduction
  {
    public int PayrollDeductionId { get; set; }
    [Precision(9, 2)]
    public decimal SdlAmount { get; set; }
    [Precision(7, 2)]
    [Range(0, (double)UIFConstants.UIFCap, ErrorMessage = "UIF cannot be greater than R17,7210.00")]
    public decimal UifEmployeeAmount { get; set; }
    [Precision(7, 2)]
    public decimal UifEmployerAmount { get; set; }
    public int EmployeeId { get; set; }
    //Nice to haves
  }
}
namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations.Schema;
  using HRConnect.Api.Models.Payroll;

  public class LunchDeduction : PayrollRecord
  {
    public int LunchDeductionId { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
  }
}

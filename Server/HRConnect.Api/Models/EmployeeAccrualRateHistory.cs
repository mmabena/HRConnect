namespace HRConnect.Api.Models
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq;
  using System.Threading.Tasks;
  public class EmployeeAccrualRateHistory
  {
    public int Id { get; set; }

    public string EmployeeId { get; set; } = string.Empty;

        public int PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;

        public decimal AnnualEntitlement { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal DailyRate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateOnly EffectiveFrom { get; set; }

        public DateOnly? EffectiveTo { get; set; }

    public DateTime CreatedDate { get; set; }

    public Employee? Employee { get; set; }

        public Position? Position { get; set; }
    }
}
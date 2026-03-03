
namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations.Schema;

    public class EmployeeAccrualRateHistory
    {
        public int Id { get; set; }

        public Guid EmployeeId { get; set; }

        public decimal AnnualEntitlement { get; set; } // e.g. 15, 18
        [Column(TypeName = "decimal(18,6)")]
        public decimal DailyRate { get; set; } // cached value

        public DateOnly EffectiveFrom { get; set; }

        public DateOnly? EffectiveTo { get; set; } // null = active

        public DateTime CreatedDate { get; set; }
        public Employee? Employee { get; set; }
    }
}
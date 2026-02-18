
namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    public class EmployeeLeaveBalance
    {
        public int Id { get; set; }

        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; } = null!;

        public decimal EntitledDays { get; set; }
        public decimal AccruedDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal RemainingDays { get; set; }
        public decimal CarryoverDays { get; set; }
        public decimal ForfeitedDays { get; set; }
        public int? LastResetYear { get; set; }
    }
}
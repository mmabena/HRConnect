namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    public class LeaveType
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Description { get; set; } = null!;

        public int? ResetMonth { get; set; }
        public int? ResetDay { get; set; }

        // Carryover (Annual only)
        public int? MaxCarryoverDays { get; set; }
        public int? CarryoverExpiryMonth { get; set; }
        public int? CarryoverExpiryDay { get; set; }
        public int? CarryoverNotificationMonth { get; set; }
        public int? CarryoverNotificationDay { get; set; }

        // Rolling window (Sick & Family)
        public bool IsRollingWindow { get; set; }
        public int? RollingMonths { get; set; }

        // Restriction (Maternity)
        public bool FemaleOnly { get; set; }

        public bool IsActive { get; set; }

        public ICollection<LeaveEntitlementRule> EntitlementRules { get; set; }
            = new List<LeaveEntitlementRule>();
    }
}
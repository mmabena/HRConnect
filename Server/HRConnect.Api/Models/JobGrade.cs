namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class JobGrade
    {
        public int JobGradeId { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public ICollection<Position> Positions { get; set; } = new List<Position>();
        public ICollection<LeaveEntitlementRule> LeaveEntitlementRules { get; set; }
           = new List<LeaveEntitlementRule>();
    }
}
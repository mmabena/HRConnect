
namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class JobGrade
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public ICollection<LeaveEntitlementRule> LeaveEntitlementRules { get; set; }
            = new List<LeaveEntitlementRule>();
    }
}
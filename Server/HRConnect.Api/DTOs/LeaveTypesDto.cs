
namespace HRConnect.Api.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    public class CreateLeaveTypeRequest
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }

        public bool FemaleOnly { get; set; }

        public List<LeaveEntitlementRuleRequest> Rules { get; set; } = new();
    }
    public class LeaveEntitlementRuleRequest
    {
        public int JobGradeId { get; set; }

        public decimal MinYearsService { get; set; }

        public decimal? MaxYearsService { get; set; }

        public decimal DaysAllocated { get; set; }
    }
    public class LeaveTypeResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Code { get; set; } = null!;

        public bool FemaleOnly { get; set; }

        public bool IsActive { get; set; }

        public List<LeaveEntitlementRuleSummary> Rules { get; set; } = new();
    }
    public class LeaveEntitlementRuleSummary
    {
        public int JobGradeId { get; set; }

        public decimal MinYearsService { get; set; }

        public decimal? MaxYearsService { get; set; }

        public decimal DaysAllocated { get; set; }
    }

}
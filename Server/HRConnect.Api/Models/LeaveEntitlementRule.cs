namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    public class LeaveEntitlementRule
    {
        public int Id { get; set; }

        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; } = null!;

        public int JobGradeId { get; set; }
        public JobGrade JobGrade { get; set; } = null!;

        public decimal MinYearsService { get; set; }
        public decimal? MaxYearsService { get; set; }

        public int DaysAllocated { get; set; }

        public bool IsActive { get; set; }
            
    }
}
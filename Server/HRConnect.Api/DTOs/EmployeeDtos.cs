namespace HRConnect.Api.DTOs
{
    using System;
    using System.Collections.Generic;
    public class CreateEmployeeRequest
    {
        public int PositionId { get; set; }
        public string ReportingManagerId { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateOnly StartDate { get; set; }
    }
    public class UpdatePositionRequest
    {
        public int NewPositionId { get; set; }
    }
    public class EmployeeResponse
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = null!;
        public string Gender { get; set; } = null!;

        public string Position { get; set; } = null!;
        public string JobGrade { get; set; } = null!;

        public DateOnly StartDate { get; set; }

        // Keep this high-level summary
        public decimal AnnualLeaveRemaining { get; set; }

        // Optional detailed balances (used in GET by ID)
        public List<LeaveBalanceSummary> LeaveBalances { get; set; } = new();
    }
    public class LeaveBalanceSummary
    {
        public string LeaveType { get; set; } = null!;
        public decimal EntitledDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal RemainingDays { get; set; }
    }
    public class UpdateLeaveRuleRequest
    {
        public int RuleId { get; set; }
        public decimal NewDaysAllocated { get; set; }
    }
    public class UpdateUsedDaysRequest
    {
        public Guid EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public decimal UsedDays { get; set; }
    }

}

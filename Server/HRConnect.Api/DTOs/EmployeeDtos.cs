namespace HRConnect.Api.DTOs
{
    using System;
    using System.Collections.Generic;

    // ============================================================
    // CREATE EMPLOYEE REQUEST
    // Used by: POST /api/Employee
    // ============================================================
    public class CreateEmployeeRequest
    {
        public int PositionId { get; set; }
        public string ReportingManagerId { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateOnly StartDate { get; set; }
    }

    // ============================================================
    // UPDATE POSITION REQUEST (Optional body version if needed later)
    // Used by: PUT /api/Employee/{id}/position
    // ============================================================
    public class UpdatePositionRequest
    {
        public int NewPositionId { get; set; }
    }

    // ============================================================
    // MAIN EMPLOYEE RESPONSE (Used everywhere)
    // GET All
    // GET By Id
    // POST Response
    // PUT Response
    // ============================================================
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

    // ============================================================
    // LEAVE BALANCE SUMMARY
    // Clean. No navigation properties.
    // ============================================================
    public class LeaveBalanceSummary
    {
        public string LeaveType { get; set; } = null!;
        public decimal EntitledDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal RemainingDays { get; set; }
    }
}

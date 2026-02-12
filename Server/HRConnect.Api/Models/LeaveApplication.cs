
namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class LeaveApplication
    {
        public int Id { get; set; }

        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public decimal DaysRequested { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime AppliedDate { get; set; }
        public DateTime? DecisionDate { get; set; }

        public string? ApprovedBy { get; set; }
    }
}
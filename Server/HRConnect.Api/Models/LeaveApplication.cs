
namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
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

        public LeaveApplicationStatus Status { get; set; }
            = LeaveApplicationStatus.Pending;

        public DateTime AppliedDate { get; set; }

        public DateTime? DecisionDate { get; set; }

        public string? DecisionBy { get; set; }
        public enum LeaveApplicationStatus
        {
            Pending = 0,
            Approved = 1,
            Rejected = 2
        }
        public string? RejectionReason { get; set; }
        public Guid ApprovalToken { get; set; } = Guid.NewGuid();

        public DateTime TokenExpiry { get; set; } = DateTime.UtcNow.AddDays(2);
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    }

}
namespace HRConnect.Api.Models
{
    /// <summary>
    /// Tracks the current available leave balance per employee and leave type.
    /// Used for fast lookups and rolling window calculations.
    /// </summary>
    public class EmployeeLeaveBalance
    {
        public int EmployeeLeaveBalanceId { get; set; }

        public int EmployeeId { get; set; }

        public int LeaveTypeId { get; set; }

        public int DaysAvailable { get; set; }

        public DateTime LastUpdated { get; set; }

        public Employee Employee { get; set; }
        public LeaveType LeaveType { get; set; }
    }
}

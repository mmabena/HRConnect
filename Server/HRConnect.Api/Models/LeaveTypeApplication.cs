namespace HRConnect.Api.Models
{
    using System;
    /// <summary>
    /// Captures leave application submitted by an employee
    /// before approval workflow starts.
    /// </summary>
    public class LeaveTypeApplication
    {
        public int LeaveTypeApplicationId { get; set; }
        public int LeaveTypeId { get; set; }
        public int EmployeeId { get; set; }
        public string Name { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public int Days { get; set; }

        public DateTime Date { get; set; }
        public Employee Employee { get; set; }
        public LeaveType LeaveType { get; set; }
    }
}
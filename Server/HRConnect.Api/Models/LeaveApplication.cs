namespace HRConnect.Api.Models
{
    using System;
    /// <summary>
    /// Represents leave requests reviewed by SuperUser / Reporting Manager.
    /// </summary>
    public class LeaveApplication
    {
        public int LeaveApplicationId { get; set; }

        public int EmployeeId { get; set; }

        public int LeaveTypeId { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public int DaysNeeded { get; set; }

        public int DaysAvailable { get; set; }

        public DateTime Date { get; set; }

        public string Status { get; set; }

        public Employee Employee { get; set; }
        public LeaveType LeaveType { get; set; }
    }
}
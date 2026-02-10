namespace HRConnect.Api.Models
{
    /// <summary>
    /// Stores calculated leave entitlements per employee.
    /// This table is system-managed and updated automatically.
    /// </summary>
    public class LeaveEntitlementRule
    {
        public int LeaveEntitlementRuleId { get; set; }
        public int LeaveTypeId { get; set; }
        public int EmployeeId { get; set; }
        public int JobGradeId { get; set; }
        public string EmployeeName { get; set; }
        public string JobGrade { get; set; }
        public int YearsOfService { get; set; }
        public int DaysEntitled { get; set; }
        public string Status { get; set; }
        public Employee Employee { get; set; }
        public LeaveType LeaveType { get; set; }
        public JobGrade JobGradeRecord { get; set; }
    }
}
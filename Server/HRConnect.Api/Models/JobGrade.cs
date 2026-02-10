namespace HRConnect.Api.Models
{
    using System;
    /// <summary>
    /// Stores job grade history for an employee.
    /// A new record is created whenever the employee is promoted or demoted.
    /// </summary>
    public class JobGrade
    {
        public int JobGradeId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string ReportingManager { get; set; }
        public string JobGradeName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public Employee Employee { get; set; }
    }
}
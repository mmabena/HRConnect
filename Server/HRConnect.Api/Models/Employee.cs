namespace HRConnect.Api.Models
{
    using System;
    /// <summary>
    /// Represents an employee in the organization.
    /// This table is the root for leave entitlement and applications.
    /// </summary>
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Gender { get; set; }
        public string ReportingManager { get; set; }
        public string JobGrade { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
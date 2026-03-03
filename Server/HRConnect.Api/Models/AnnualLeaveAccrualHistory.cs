namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class AnnualLeaveAccrualHistory
    {
        public int Id { get; set; }                     // PK (int is fine)

        public Guid EmployeeId { get; set; }            // FK → Employee

        public int Year { get; set; }                   // Closed year (e.g. 2025)

        public decimal OpeningBalance { get; set; }     // Carryover from previous year
        public decimal Accrued { get; set; }            // Total accrued during year
        public decimal Used { get; set; }               // Total used during year
        public decimal Forfeited { get; set; }          // Lost at reset
        public decimal ClosingBalance { get; set; }     // Balance at 31 Dec

        public DateTime CreatedDate { get; set; }

        // Navigation
        public Employee? Employee
        {
            get; set;
        }
    }
}
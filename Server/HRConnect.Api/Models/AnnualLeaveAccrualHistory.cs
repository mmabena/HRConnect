namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class AnnualLeaveAccrualHistory
    {
        public int Id { get; set; }                     

        public string EmployeeId { get; set; }            
        public int Year { get; set; }                   

        public decimal OpeningBalance { get; set; }     
        public decimal Accrued { get; set; }            
        public decimal Used { get; set; }               
        public decimal Forfeited { get; set; }          
        public decimal ClosingBalance { get; set; }     

        public DateTime CreatedDate { get; set; }

        // Navigation
        public Employee? Employee
        {
            get; set;
        }
    }
}
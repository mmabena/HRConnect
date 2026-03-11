namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    public enum Gender
    {
        Male,
        Female
    }
    public class Employee
    {
        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        public int PositionId { get; set; }
        public Position? Position { get; set; }

        public string? CareerManagerID { get; set; }

        [ForeignKey(nameof(CareerManagerID))]
        public Employee? CareerManager { get; set; }

        public ICollection<Employee>? Subordinates { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public Gender Gender { get; set; }

        public DateOnly StartDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public ICollection<EmployeeLeaveBalance> LeaveBalances { get; set; }
            = new List<EmployeeLeaveBalance>();

        public ICollection<LeaveApplication> LeaveApplications { get; set; }
            = new List<LeaveApplication>();

        public ICollection<EmployeeAccrualRateHistory> AccrualRateHistory { get; set; }
            = new List<EmployeeAccrualRateHistory>();

        public ICollection<AnnualLeaveAccrualHistory> AnnualLeaveAccrualHistories { get; set; }
            = new List<AnnualLeaveAccrualHistory>();
    }
}
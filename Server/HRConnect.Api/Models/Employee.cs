
namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Employee
    {
        public Guid EmployeeId { get; set; }

        public int PositionId { get; set; }
        public Position Position { get; set; } = null!;

        public string ReportingManagerId { get; set; } = null!;

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Gender { get; set; } = null!;

        public DateOnly StartDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public ICollection<EmployeeLeaveBalance> LeaveBalances { get; set; }
            = new List<EmployeeLeaveBalance>();

        public ICollection<LeaveApplication> LeaveApplications { get; set; }
            = new List<LeaveApplication>();
    }
}
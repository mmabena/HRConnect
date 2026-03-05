
namespace HRConnect.Api.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    public class CreateApplicationRequest
    {
        public Guid EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? Description { get; set; }
    }
    public class LeaveApplicationResponse
    {
        public int Id { get; set; }
        public Guid EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal DaysRequested { get; set; }
        public string Status { get; set; } = null!;
    }
}
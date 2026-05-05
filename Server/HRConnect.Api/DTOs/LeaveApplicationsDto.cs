namespace HRConnect.Api.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    public class CreateApplicationRequest
    {
        public string EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? Description { get; set; }

        public List<IFormFile>? Documents { get; set; }
    }
    public class LeaveApplicationResponse
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal DaysAllocated { get; set; }
        public decimal DaysRequested { get; set; }
        public string Status { get; set; } = null!;
        public List<LeaveDocumentResponse> Documents { get; set; } = new();
    }
    public class DecisionRequest
    {
        public string? Reason { get; set; } = string.Empty;
    }
    public class LeaveDocumentResponse
    {
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
    }
}
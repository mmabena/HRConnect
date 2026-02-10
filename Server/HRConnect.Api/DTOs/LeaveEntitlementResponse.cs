
namespace HRConnect.Api.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class LeaveEntitlementResponse
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeSurname { get; set; } = string.Empty;
        public string LeaveType { get; set; } = string.Empty;
        public int DaysEntitled { get; set; }
        public int DaysAvailable { get; set; }
    }
}
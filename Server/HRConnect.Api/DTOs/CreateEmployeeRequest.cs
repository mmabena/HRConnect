namespace HRConnect.Api.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    public class CreateEmployeeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string ReportingManager { get; set; } = string.Empty;
        public string JobGradeName { get; set; } = string.Empty;

    }
}
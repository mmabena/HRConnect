namespace HRConnect.Api.DTOs.UserCompany
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    public class UserCompanyDto
    {
        public string CompanyId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        // Original company user was created to.
        public bool IsOriginalCompany { get; set; }
        //EMployee Count for company management page frontend
        public int EmployeeCount { get; set; }
    }
}
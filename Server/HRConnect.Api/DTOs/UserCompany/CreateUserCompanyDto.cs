using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRConnect.Api.DTOs.UserCompany
{
    public class CreateUserCompanyDto
    {
        public string CompanyId { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}
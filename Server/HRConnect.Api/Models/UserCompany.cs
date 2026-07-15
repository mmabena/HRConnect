namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using HRConnect.Api.Models;
    using System.Linq;
    using System.Threading.Tasks;
    public class UserCompany
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string CompanyId { get; set; } = string.Empty;
        public Company Company { get; set; } = null!;
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
namespace HRConnect.Api.DTOs.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    public class CurrentUserDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string? FullName { get; set; }
        public string? JobTitle { get; set; }

    }
}
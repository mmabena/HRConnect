namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    public class Position
    {
        public int PositionId { get; set; }
        public string PositionTitle { get; set; } = string.Empty;

        public List<Employee> Employees { get; set; } = new();
    }
}


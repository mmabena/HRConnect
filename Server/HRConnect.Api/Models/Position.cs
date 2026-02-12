
namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;

    public class Position
    {
        public int PositionId { get; set; }

        public string Title { get; set; } = null!;

        public int JobGradeId { get; set; }
        public JobGrade JobGrade { get; set; } = null!;

        public ICollection<Employee> Employees { get; set; }
            = new List<Employee>();
    }
}
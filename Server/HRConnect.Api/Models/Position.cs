namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Position
    {
        public int PositionId { get; set; }

        public string PositionTitle { get; set; } = string.Empty;

        public int JobGradeId { get; set; }
        [ForeignKey(nameof(JobGradeId))]
        public JobGrade JobGrade { get; set; } = null!;

        public int OccupationalLevelId { get; set; }
        [ForeignKey(nameof(OccupationalLevelId))]
        public OccupationalLevel OccupationalLevels { get; set; } = null!;

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public bool IsActive { get; set; }

        public List<Employee> Employees { get; set; } = new();
    }
}
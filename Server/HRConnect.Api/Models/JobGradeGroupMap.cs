namespace HRConnect.Api.Models
{
    public class JobGradeGroupMap
    {
        public int Id { get; set; }

        public int JobGradeId { get; set; }
        public JobGrade JobGrade { get; set; } = null!;

        public string GroupKey { get; set; } = null!;
    }
}
namespace HRConnect.Api.DTOs
{
    public class JobGradeGroupResponse
    {
        public string GroupKey { get; set; } = null!;
        public List<JobGradeDto> JobGrades { get; set; } = new();
    }

    public class JobGradeDto
    {
        public int JobGradeId { get; set; }
        public string Name { get; set; } = null!;
    }

    public class CreateGroupRequest
    {
        public string GroupKey { get; set; } = null!;
        public List<int> JobGradeIds { get; set; } = new();
    }

    public class MoveJobGradeRequest
    {
        public int JobGradeId { get; set; }
        public string NewGroupKey { get; set; } = null!;
    }

    public class RemoveJobGradeRequest
    {
        public int JobGradeId { get; set; }
    }
}
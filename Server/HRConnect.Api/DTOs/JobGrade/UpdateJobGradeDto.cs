namespace HRConnect.Api.DTOs.JobGrade
{
using HRConnect.Api.Models;
    public class UpdateJobGradeDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
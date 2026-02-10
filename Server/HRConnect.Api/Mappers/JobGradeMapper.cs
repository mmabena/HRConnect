namespace HRConnect.Api.Mappers
{
  using System.Collections.Generic;
  using HRConnect.Api.DTOs.Position;
    public static class JobGradeMapper
    {
        public static JobGradeDto ToJobGradeDto(this JobGrade jobGrade)
        {
        return new JobGradeDto
        {
            Name = jobGrade.Name,
            Description = jobGrade.Description,
            IsActive = jobGrade.IsActive
        };
        }
    }
}
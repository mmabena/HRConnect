namespace HRConnect.Api.Mappers
{
    using HRConnect.Api.Models;
  using System.Collections.Generic;
  using HRConnect.Api.DTOs.JobGrade;
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
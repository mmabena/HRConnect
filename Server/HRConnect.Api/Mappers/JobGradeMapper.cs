namespace HRConnect.Api.Mappers
{
    using System.Collections.Generic;
  using HRConnect.Api.DTOs.JobGrade;
  using HRConnect.Api.Models;
    public static class JobGradeMapper
    {

        public static JobGrade ToJobGradeDto(this CreateJobGradeDto createJobGradeDto)
        {
            return new JobGrade
            {
                Name = createJobGradeDto.Name,
                IsActive = createJobGradeDto.IsActive,
                CreatedDate = DateTime.UtcNow
            };
        }
        public static JobGradeDto ToJobGradeDto(this JobGrade jobGrade)
        {
        return new JobGradeDto
        {
            JobGradeId = jobGrade.JobGradeId,   
            Name = jobGrade.Name,
            IsActive = jobGrade.IsActive,
            CreatedDate = jobGrade.CreatedDate,
            UpdatedDate = jobGrade.UpdatedDate
        };
        }
    }
}
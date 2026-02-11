namespace HRConnect.Api.Mappers
{
    using System;
    using Api.DTOs.Position;
    using Api.Models;
    using HRConnect.Api.DTOs.JobGrade;
    using HRConnect.Api.DTOs.OccupationalLevel;

    public static class PositionMapper
    {
        public static Position ToPosition(this CreatePositionDto createPositionDto)
        {
            return new Position
            {
                Title = createPositionDto.Title,
                JobGradeId = createPositionDto.JobGradeId,
                OccupationalLevelId = createPositionDto.OccupationalLevelId,
                IsActive = createPositionDto.IsActive,
                CreatedDate = DateTime.UtcNow
            };
        }

        public static ReadPositionDto ToReadPositionDto(this Position position)
        {
            return new ReadPositionDto
            {
                PositionId = position.PositionId,
                Title = position.Title,
                JobGradeId = position.JobGradeId,
                OccupationalLevelId = position.OccupationalLevelId,
                CreatedDate = position.CreatedDate,
                UpdatedDate = position.UpdatedDate,
                IsActive = position.IsActive,
                JobGrade = position.JobGrade != null ? new JobGradeDto
                {
                    Name = position.JobGrade.Name,
                    IsActive = position.JobGrade.IsActive
                } : null,
               OccupationalLevel = position.OccupationalLevels != null ? new OccupationalLevelDto
               {

                 Description = position.OccupationalLevels.Description,
                } : null

            };
        }
    }
}
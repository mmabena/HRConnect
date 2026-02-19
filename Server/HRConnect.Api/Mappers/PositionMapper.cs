namespace HRConnect.Api.Mappers
{
    using System;
    using Api.DTOs.Position;
    using Api.Models;
    using HRConnect.Api.DTOs.JobGrade;
    using HRConnect.Api.DTOs.OccupationalLevel;

    public static class PositionMapper
    {
        public static Position ToPositionDto(this CreatePositionDto createPositionDto)
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

        public static PositionDto ToPositionDto(this Position position)
        {
           return new PositionDto
    {
        PositionId = position.PositionId,
        Title = position.Title,
        JobGradeId = position.JobGradeId,
        OccupationalLevelId = position.OccupationalLevelId,
        CreatedDate = position.CreatedDate,
        UpdatedDate = position.UpdatedDate,
        IsActive = position.IsActive,

       JobGrade = position.JobGrade?.ToJobGradeDto(),
    OccupationalLevel = position.OccupationalLevels?.ToOccupationalLevelDto()

    };
        }
    }
}
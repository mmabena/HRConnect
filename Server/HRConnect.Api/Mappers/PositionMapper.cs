namespace HRConnect.Api.Mappers
{
    using System;
    using Api.DTOs.Position;
    using Api.Models;

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
                Id = position.Id,
                Title = position.Title,
                JobGradeId = position.JobGradeId,
                OccupationalLevelId = position.OccupationalLevelId,
                CreatedDate = position.CreatedDate,
                UpdatedDate = position.UpdatedDate,
                IsActive = position.IsActive,
                JobGrade = position.JobGrade != null ? new JobGradeDto
                {
                    Name = position.JobGrade.Name,
                    Description = position.JobGrade.Description,
                    IsActive = position.JobGrade.IsActive
                } : null,
                OccupationalLevel = position.OccupationalLevel != null ? new OccupationalLevelDto
                {
                    Name = position.OccupationalLevel.Name,
                    Description = position.OccupationalLevel.Description
                } : null
            };
        }
    }
}
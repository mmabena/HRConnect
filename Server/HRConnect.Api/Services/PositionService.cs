namespace HRConnect.Api.Services
{
    using HRConnect.Api.DTOs.Position;
    using HRConnect.Api.DTOs.JobGrade;
    using HRConnect.Api.DTOs.OccupationalLevel;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepo;

        public PositionService(IPositionRepository positionRepo)
        {
            _positionRepo = positionRepo;
        }

        // ----------------------
        // GET ALL
        // ----------------------
        public async Task<IEnumerable<ReadPositionDto>> GetAllPositionsAsync()
        {
          var positions = await _positionRepo.GetAllPositionsAsync();
            return positions.Select(MapToReadDto).ToList();
        }

        // ----------------------
        // GET BY ID
        // ----------------------
        public async Task<ReadPositionDto?> GetPositionByIdAsync(int id)
        {
            var position = await _positionRepo.GetPositionByIdAsync(id);
            if (position == null) return null;
            return MapToReadDto(position);
        }

        // ----------------------
        // GET /api/positions/title
        // ----------------------
        public async Task<ReadPositionDto?> GetPositionByTitleAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty.");

            var position = await _positionRepo.GetPositionByTitleAsync(title);
            if (position == null) return null;

            return MapToReadDto(position);
        }

        // ----------------------
        // post/Aapi/positions
        // ----------------------
        public async Task<ReadPositionDto> CreatePositionAsync(CreatePositionDto createPositionDto)
        {
            ValidateCreateDto(createPositionDto);
            await EnsureUniqueTitle(createPositionDto.Title);

            var position = new Position
            {
                Title = createPositionDto.Title,
                JobGradeId = createPositionDto.JobGradeId,
                OccupationalLevelId = createPositionDto.OccupationalLevelId,
                IsActive = createPositionDto.IsActive,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var created = await _positionRepo.CreatePositionAsync(position);
            return MapToReadDto(created);
        }

        // ----------------------
        // UPDATE
        // ----------------------
        public async Task<ReadPositionDto?> UpdatePositionAsync(int id, UpdatePositionDto updatePositionDto)
        {
            var position = await _positionRepo.GetPositionByIdAsync(id);
            if (position == null)
                throw new KeyNotFoundException($"Position with ID {id} not found.");

            ValidateUpdateDto(updatePositionDto);
            await EnsureUniqueTitle(updatePositionDto.Title, id);

            // Update properties
            position.Title = updatePositionDto.Title;
            position.JobGradeId = updatePositionDto.JobGradeId;
            position.OccupationalLevelId = updatePositionDto.OccupationalLevelId;
            position.IsActive = updatePositionDto.IsActive;
            position.UpdatedDate = DateTime.UtcNow;

            var updated = await _positionRepo.UpdatePositionAsync(id, position);
            return updated == null ? null : MapToReadDto(updated);
        }

        // ----------------------
        // PRIVATE HELPERS
        // ----------------------
        private static ReadPositionDto MapToReadDto(Position p)
        {
            return new ReadPositionDto
            {
                PositionId = p.PositionId,
                Title = p.Title,
                JobGradeId = p.JobGradeId,
                OccupationalLevelId = p.OccupationalLevelId,
                CreatedDate = p.CreatedDate,
                UpdatedDate = p.UpdatedDate,
                IsActive = p.IsActive,
                JobGrade = p.JobGrade != null ? new JobGradeDto
                {
                    Name = p.JobGrade.Name,
                    IsActive = p.JobGrade.IsActive
                } : null,
                OccupationalLevel = p.OccupationalLevels != null ? new OccupationalLevelDto
                {
                    Description = p.OccupationalLevels.Description
                } : null
            };
        }

        private static void ValidateCreateDto(CreatePositionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Position title is required.");
        }

        private static void ValidateUpdateDto(UpdatePositionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Position title is required.");
        }

        private async Task EnsureUniqueTitle(string title, int? excludeId = null)
        {
            var exists = await _positionRepo.TitleExistsAsync(title, excludeId ?? 0);
            if (exists)
                throw new ArgumentException("A position with this title already exists.");
        }
    }
}

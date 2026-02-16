namespace HRConnect.Api.Services
{
    using HRConnect.Api.DTOs.JobGrade;
    using HRConnect.Api.DTOs.OccupationalLevel;
    using HRConnect.Api.DTOs.Position;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Mappers;
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
        public async Task<List<PositionDto>> GetAllPositionsAsync()
        {
          var positions = await _positionRepo.GetAllPositionsAsync();
            return positions 
                .OrderBy(p => p.PositionId)
                .Select(p => p.ToPositionDto())
                .ToList();
        }

        // ----------------------
        // GET BY ID
        // ----------------------
        public async Task<PositionDto?> GetPositionByIdAsync(int id)
        {
            var position = await _positionRepo.GetPositionByIdAsync(id);
            if (position == null) return null;
            return MapToPositionDto(position);
        }

        // ----------------------
        // GET /api/positions/title
        // ----------------------
        public async Task<PositionDto?> GetPositionByTitleAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty.");

            var position = await _positionRepo.GetPositionByTitleAsync(title);
            if (position == null) return null;

            return MapToPositionDto(position);
        }

        // ----------------------
        // post/Aapi/positions
        // ----------------------
        public async Task<PositionDto> AddPositionAsync(CreatePositionDto createPositionDto)
        {
            ValidateCreateDto(createPositionDto);
            await EnsureUniqueTitle(createPositionDto.Title);
            
            //Prevents duplicate titles
            var existing = await _positionRepo.GetPositionByTitleAsync(createPositionDto.Title);
            if (existing != null)
                throw new InvalidOperationException("A position with this name already exists.");

            var position = new Position
            {
                Title = createPositionDto.Title,
                JobGradeId = createPositionDto.JobGradeId,
                OccupationalLevelId = createPositionDto.OccupationalLevelId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsActive = createPositionDto.IsActive
            };

            var created = await _positionRepo.AddPositionAsync(position);
            return MapToPositionDto(created);
        }

        // ----------------------
        // UPDATE
        // ----------------------
        public async Task<PositionDto?> UpdatePositionAsync(int id, UpdatePositionDto updatePositionDto)
        {
            var position = await _positionRepo.GetPositionByIdAsync(id);
            if (position == null)
                throw new KeyNotFoundException($"Position with ID {id} not found.");

            ValidateUpdateDto(updatePositionDto);
            await EnsureUniqueTitle(updatePositionDto.Title, id);

            var jobGrade = await _positionRepo.GetPositionByIdAsync(updatePositionDto.JobGradeId);
            if (jobGrade == null)
                throw new KeyNotFoundException($"JobGrade with ID {updatePositionDto.JobGradeId} not found.");

            if(!jobGrade.IsActive)
                throw new InvalidOperationException("Cannot assign a position to an inactive Job Grade.");
            
            var occupationalLevel = await _positionRepo.GetPositionByIdAsync(updatePositionDto.OccupationalLevelId);
            if (occupationalLevel == null)
                throw new KeyNotFoundException($"OccupationalLevel with ID {updatePositionDto.OccupationalLevelId} not found.");
            
            if(!occupationalLevel.IsActive)
                throw new InvalidOperationException("Occupational level does not exist.");

            // Update properties
            position.Title = updatePositionDto.Title;
            position.JobGradeId = updatePositionDto.JobGradeId;
            position.OccupationalLevelId = updatePositionDto.OccupationalLevelId;
            position.IsActive = updatePositionDto.IsActive;
            position.UpdatedDate = DateTime.UtcNow;

            var updated = await _positionRepo.UpdatePositionAsync(id, position);
            return updated == null ? null : MapToPositionDto(updated);
        }

        // ----------------------
        // PRIVATE HELPERS
        // ----------------------
        private static PositionDto MapToPositionDto(Position p)
        {
            return new PositionDto
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

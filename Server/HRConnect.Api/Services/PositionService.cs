namespace HRConnect.Api.Services
{
    using HRConnect.Api.DTOs.JobGrade;
    using HRConnect.Api.DTOs.OccupationalLevel;
    using HRConnect.Api.DTOs.Position;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Mappers;
    using HRConnect.Api.Models;
    using HRConnect.Api.Utils;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepo;
        private readonly IJobGradeRepository _jobGradeRepo;
        private readonly IOccupationalLevelRepository _occupationalLevelRepo;

        public PositionService(IPositionRepository positionRepo, IJobGradeRepository jobGradeRepo, IOccupationalLevelRepository occupationalLevelRepo)
        {
            _positionRepo = positionRepo;
            _jobGradeRepo = jobGradeRepo;
            _occupationalLevelRepo = occupationalLevelRepo;
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

            var position = new Position
            {
                PositionTitle = createPositionDto.PositionTitle.Trim(),
                JobGradeId = createPositionDto.JobGradeId,
                OccupationalLevelId = createPositionDto.OccupationalLevelId,
                IsActive = createPositionDto.IsActive,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            try
            {
                var created = await _positionRepo.AddPositionAsync(position);
                return MapToPositionDto(created);
            }
            catch (DbUpdateException ex)
                when (DbExceptionHelper.IsUniqueConstraintViolation(ex))
            {
                throw new DomainException("A position with this title already exists.");
            }
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

            if (updatePositionDto.JobGradeId > 0)
            {
                var jobGrade = await _jobGradeRepo.GetJobGradeByIdAsync(updatePositionDto.JobGradeId);

                if (jobGrade == null)
                    throw new KeyNotFoundException($"JobGrade with ID {updatePositionDto.JobGradeId} not found.");

                if (!jobGrade.IsActive)
                    throw new InvalidOperationException("Cannot assign a position to an inactive Job Grade.");

                position.JobGradeId = updatePositionDto.JobGradeId;
            }


            if (updatePositionDto.OccupationalLevelId > 0)
            {
                var occupationalLevel = await _occupationalLevelRepo.GetOccupationalLevelByIdAsync(updatePositionDto.OccupationalLevelId);
                if (occupationalLevel == null)
                    throw new KeyNotFoundException($"OccupationalLevel with ID {updatePositionDto.OccupationalLevelId} not found.");

                if (!occupationalLevel.IsActive)
                    throw new InvalidOperationException("Occupational level does not exist.");

                position.OccupationalLevelId = updatePositionDto.OccupationalLevelId;
            }

            position.PositionTitle = updatePositionDto.PositionTitle;
            position.JobGradeId = updatePositionDto.JobGradeId;
            position.OccupationalLevelId = updatePositionDto.OccupationalLevelId;
            position.IsActive = updatePositionDto.IsActive;
            position.UpdatedDate = DateTime.UtcNow;


            try
            {
                var updated = await _positionRepo.UpdatePositionAsync(id, position);
                return updated == null ? null : MapToPositionDto(updated);
            }
            catch (DbUpdateException ex)
                when (DbExceptionHelper.IsUniqueConstraintViolation(ex))
            {
                throw new DomainException("A position with this title already exists.");
            }
        }


        // ----------------------
        // PRIVATE HELPERS
        // ----------------------
        private static PositionDto MapToPositionDto(Position p)
        {
            return new PositionDto
            {
                PositionId = p.PositionId,
                PositionTitle = p.PositionTitle,
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
            if (string.IsNullOrWhiteSpace(dto.PositionTitle))
                throw new ArgumentException("Position title is required.");
        }

        private static void ValidateUpdateDto(UpdatePositionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PositionTitle))
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
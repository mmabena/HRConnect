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
        /// <summary>
        /// Handles business logic related to position operations
        /// This service acts as a bridge between the controller and Repository layers.
        /// This is responsible for validation, mapping, and enforcing business rules.
        /// </summary>
        private readonly IPositionRepository _positionRepo;
        private readonly IJobGradeRepository _jobGradeRepo;
        private readonly IOccupationalLevelRepository _occupationalLevelRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionService"/> class.
        /// </summary>
        /// <param name="positionRepo">Repository for position data access.</param>
        /// <param name="jobGradeRepo">Repository for job grade data access.</param>
        /// <param name="occupationalLevelRepo">Repository for occupational level data access.</param>
        public PositionService(IPositionRepository positionRepo, IJobGradeRepository jobGradeRepo, IOccupationalLevelRepository occupationalLevelRepo)
        {
            _positionRepo = positionRepo;
            _jobGradeRepo = jobGradeRepo;
            _occupationalLevelRepo = occupationalLevelRepo;
        }


        /// <summary>
        /// Retrieves all positions from the system.
        /// </summary>
        /// <returns>A list of all positions ordered by Position ID.</returns>
        public async Task<List<PositionDto>> GetAllPositionsAsync()
        {
            var positions = await _positionRepo.GetAllPositionsAsync();
            return positions
                .OrderBy(p => p.PositionId)
                .Select(p => p.ToPositionDto())
                .ToList();
        }


        /// <summary>
        /// Retrieves all positions from the system.
        /// </summary>
        /// <returns>A list of all positions ordered by Position ID.</returns>
        public async Task<PositionDto?> GetPositionByIdAsync(int id)
        {
            var position = await _positionRepo.GetPositionByIdAsync(id);
            if (position == null) return null;
            return MapToPositionDto(position);
        }


        /// <summary>
        /// Retrieves a position by its title.
        /// </summary>
        /// <param name="title">The title of the position.</param>
        /// <returns>The position if found; otherwise null.</returns>
        /// <exception cref="ArgumentException">Thrown when the title is null or empty.</exception>
        public async Task<PositionDto?> GetPositionByTitleAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty.");

            var position = await _positionRepo.GetPositionByTitleAsync(title);
            if (position == null) return null;

            return MapToPositionDto(position);
        }


        /// <summary>
        /// Creates a new position after validating input data.
        /// </summary>
        /// <param name="createPositionDto">The data required to create a position.</param>
        /// <returns>The newly created position.</returns>
        /// <exception cref="ArgumentException">Thrown when required fields are invalid.</exception>
        /// <exception cref="DomainException">Thrown when a position with the same title already exists.</exception>
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



        /// <summary>
        /// Updates an existing position after validating input and business rules.
        /// </summary>
        /// <param name="id">The position identifier.</param>
        /// <param name="updatePositionDto">The updated position data.</param>
        /// <returns>The updated position if successful; otherwise null.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the position does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown when assigning invalid or inactive related entities.</exception>
        /// <exception cref="DomainException">Thrown when a duplicate position title exists.</exception>
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



        /// <summary>
        /// Maps a Position entity to a PositionDto.
        /// </summary>
        /// <param name="p">The position entity.</param>
        /// <returns>A mapped PositionDto object.</returns>
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

        /// <summary>
        /// Validates the input data for updating a position.
        /// </summary>
        /// <param name="dto">The update position DTO.</param>
        /// <exception cref="ArgumentException">Thrown when required fields are missing or invalid.</exception>
        private static void ValidateCreateDto(CreatePositionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PositionTitle))
                throw new ArgumentException("Position title is required.");
        }

        /// <summary>
        /// Ensures that the position title is unique.
        /// </summary>
        /// <param name="title">The position title.</param>
        /// <param name="excludeId">Optional ID to exclude during update.</param>
        /// <exception cref="ArgumentException">Thrown when a duplicate title exists.</exception>
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
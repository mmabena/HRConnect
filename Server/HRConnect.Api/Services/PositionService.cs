namespace HRConnect.Api.Services
{
    using HRConnect.Api.DTOs.Position;
    using HRConnect.Api.DTOs.JobGrade;
    using HRConnect.Api.DTOs.OccupationalLevel;
    using HRConnect.Api.Data; // Keep AppDbContext reference
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models; // Position entity
    using Microsoft.EntityFrameworkCore;
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

     public async Task<IEnumerable<ReadPositionDto>> GetAllPositionsAsync()
{
    var positions = await _positionRepo.GetAllPositionsAsync(); // await first
    return positions.Select(p => new ReadPositionDto
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
            Description = p.JobGrade.Description,
            IsActive = p.JobGrade.IsActive
        } : null,
        OccupationalLevel = p.OccupationalLevels != null ? new OccupationalLevelDto
        {
            Name = p.OccupationalLevels.Name,
            Description = p.OccupationalLevels.Description,
        } : null
    }).ToList(); 
}


        public async Task<ReadPositionDto?> GetPositionByIdAsync(int id)
        {
            var position = await _positionRepo.GetPositionByIdAsync(id);

            if (position == null) return null;

            return new ReadPositionDto
            {
                PositionId = position.PositionId,
                Title = position.Title,
                JobGrade = position.JobGrade != null ? new JobGradeDto
                {
                    Name = position.JobGrade.Name,
                    Description = position.JobGrade.Description,
                    IsActive = position.JobGrade.IsActive
                } : null,
                OccupationalLevel = position.OccupationalLevels != null ? new OccupationalLevelDto
                {
                    Name = position.OccupationalLevels.Name,
                    Description = position.OccupationalLevels.Description,
                } : null
            };
        }
public async Task<ReadPositionDto?> GetPositionByTitleAsync(string title)
{
    if (string.IsNullOrWhiteSpace(title))
        return null;

    // Get the position from the repository
    var position = await _positionRepo.GetPositionByTitleAsync(title);

    if (position == null)
        return null;

    // Map to ReadPositionDto
    var dto = new ReadPositionDto
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
            Description = position.JobGrade.Description,
            IsActive = position.JobGrade.IsActive
        } : null!,
        OccupationalLevel = position.OccupationalLevels != null ? new OccupationalLevelDto
        {
            Name = position.OccupationalLevels.Name,
            Description = position.OccupationalLevels.Description
        } : null!
    };

    return dto;
}

public async Task<ReadPositionDto> CreatePositionAsync(CreatePositionDto createPositionDto)
{
    var position = new Position
    {
        Title = createPositionDto.Title,
        JobGradeId = createPositionDto.JobGradeId,
        OccupationalLevelId = createPositionDto.OccupationalLevelId,
        IsActive = createPositionDto.IsActive,
        CreatedDate = DateTime.UtcNow,
        UpdatedDate = DateTime.UtcNow
    };

    var createdPosition = await _positionRepo.CreatePositionAsync(position);

    return new ReadPositionDto
    {
        PositionId = createdPosition.PositionId,
        Title = createdPosition.Title,
        JobGradeId = createdPosition.JobGradeId,
        OccupationalLevelId = createdPosition.OccupationalLevelId,
        CreatedDate = createdPosition.CreatedDate,
        UpdatedDate = createdPosition.UpdatedDate,
        IsActive = createdPosition.IsActive,
        JobGrade = createdPosition.JobGrade != null ? new JobGradeDto
        {
            Name = createdPosition.JobGrade.Name,
            Description = createdPosition.JobGrade.Description,
            IsActive = createdPosition.JobGrade.IsActive
        } : null!,
        OccupationalLevel = createdPosition.OccupationalLevels != null ? new OccupationalLevelDto
        {
            Name = createdPosition.OccupationalLevels.Name,
            Description = createdPosition.OccupationalLevels.Description
        } : null!
    };
}
public async Task<ReadPositionDto?> UpdatePositionAsync(int id, UpdatePositionDto updatePositionDto)
{
    var position = await _positionRepo.GetPositionByIdAsync(id);

    if (position == null) return null;

    // Update only existing properties
    position.Title = updatePositionDto.Title;
    position.JobGradeId = updatePositionDto.JobGradeId;
    position.OccupationalLevelId = updatePositionDto.OccupationalLevelId;
    position.IsActive = updatePositionDto.IsActive;
    position.UpdatedDate = DateTime.UtcNow;

    // Pass both id and position to match the repository method signature
    var updatedPosition = await _positionRepo.UpdatePositionAsync(id, position);

    if (updatedPosition == null) return null;

    return new ReadPositionDto
    {
        PositionId = updatedPosition.PositionId,
        Title = updatedPosition.Title,
        JobGradeId = updatedPosition.JobGradeId,
        OccupationalLevelId = updatedPosition.OccupationalLevelId,
        CreatedDate = updatedPosition.CreatedDate,
        UpdatedDate = updatedPosition.UpdatedDate,
        IsActive = updatedPosition.IsActive,
        JobGrade = updatedPosition.JobGrade != null ? new JobGradeDto
        {
            Name = updatedPosition.JobGrade.Name,
            Description = updatedPosition.JobGrade.Description,
            IsActive = updatedPosition.JobGrade.IsActive
        } : null!,
        OccupationalLevel = updatedPosition.OccupationalLevels != null ? new OccupationalLevelDto
        {
            Name = updatedPosition.OccupationalLevels.Name,
            Description = updatedPosition.OccupationalLevels.Description
        } : null!
    };
}
        public async Task<bool> DeletePositionAsync(int id)
        {
            var position = await _positionRepo.GetPositionByIdAsync(id);

            if (position == null) return false;

            await _positionRepo.DeletePositionAsync(id);
      

            return true;
        }
    }
}

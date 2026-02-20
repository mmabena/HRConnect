namespace HRConnect.Api.Services
{
    using HRConnect.Api.DTOs.OccupationalLevel;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Mappers;
    using HRConnect.Api.Models;
    using HRConnect.Api.Utils;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

  public class OccupationalLevelService : IOccupationalLevelService
  {
    private readonly IOccupationalLevelRepository _occupationalLevelRepo;

    public OccupationalLevelService(IOccupationalLevelRepository occupationalLevelRepo)
    {
      _occupationalLevelRepo = occupationalLevelRepo;
    }

    public async Task<List<OccupationalLevelDto>> GetAllOccupationalLevelsAsync()
    {
      var occupationalLevels = await _occupationalLevelRepo.GetAllOccupationalLevelsAsync();
      return occupationalLevels
        .OrderBy(ol => ol.OccupationalLevelId)
        .Select(ol => ol.ToOccupationalLevelDto())
        .ToList();
    }

    public async Task<OccupationalLevelDto?> GetOccupationalLevelByIdAsync(int id)
    {
     
            if (id <= 0)
                throw new ArgumentException("OccupationalLevelId must exist", nameof(id));

            var occupationalLevel = await _occupationalLevelRepo.GetOccupationalLevelByIdAsync(id);

            if (occupationalLevel is null)
                throw new KeyNotFoundException($"OccupationalLevel with Id {id} does not exist.");

            return occupationalLevel.ToOccupationalLevelDto();
    }

    public async Task<OccupationalLevelDto> AddOccupationalLevelAsync(CreateOccupationalLevelDto createOccupationalLevelDto)
    {
        
          ArgumentNullException.ThrowIfNull(createOccupationalLevelDto);

        if (string.IsNullOrWhiteSpace(createOccupationalLevelDto.Description))
          throw new ArgumentException("Description is required.");

        var trimmedDescription = createOccupationalLevelDto.Description.Trim();

        // Prevent duplicate
        var existing = await _occupationalLevelRepo.GetOccupationalLevelByDescriptionAsync(trimmedDescription);
        if (existing != null)
          throw new InvalidOperationException("Occupational level already exists.");

        var occupationalLevel = new OccupationalLevel
        {
          Description = trimmedDescription,
          CreatedDate = DateTime.UtcNow,
          UpdatedDate = DateTime.UtcNow
        };

        await _occupationalLevelRepo.AddOccupationalLevelAsync(occupationalLevel);
        
        return occupationalLevel.ToOccupationalLevelDto();
    }

public async Task<OccupationalLevelDto?> UpdateOccupationalLevelAsync(
    int id,
    UpdateOccupationalLevelDto updateOccupationalLevelDto)
{
    ArgumentNullException.ThrowIfNull(updateOccupationalLevelDto);

    var occupationalLevel = await _occupationalLevelRepo
        .GetOccupationalLevelByIdAsync(id);

    if (occupationalLevel == null)
        return null;

    // ✅ ADD THIS DUPLICATE CHECK
    if (!string.IsNullOrWhiteSpace(updateOccupationalLevelDto.Description))
    {
        var trimmedDescription = updateOccupationalLevelDto.Description.Trim();

        var duplicate = await _occupationalLevelRepo
            .GetOccupationalLevelByDescriptionAsync(trimmedDescription);

        if (duplicate != null && duplicate.OccupationalLevelId != id)
        {
            throw new DomainException(
                "An occupational level with this description already exists.");
        }

        occupationalLevel.Description = trimmedDescription;
    }

    if (updateOccupationalLevelDto.IsActive.HasValue)
    {
        occupationalLevel.IsActive =
            updateOccupationalLevelDto.IsActive.Value;
    }

    occupationalLevel.UpdatedDate = DateTime.UtcNow;

    try
    {
        await _occupationalLevelRepo
            .UpdateOccupationalLevelAsync(occupationalLevel);

        return occupationalLevel.ToOccupationalLevelDto();
    }
    catch (DbUpdateException ex)
        when (DbExceptionHelper.IsUniqueConstraintViolation(ex))
    {
        throw new DomainException(
            "An occupational level with this description already exists.");
    }
}



}
}

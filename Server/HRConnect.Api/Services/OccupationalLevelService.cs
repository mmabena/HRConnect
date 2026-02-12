namespace HRConnect.Api.Services
{
    using HRConnect.Api.DTOs.OccupationalLevel;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Mappers;
    using HRConnect.Api.Models;
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
      .OrderBy(ol => ol.Description)
      .Select(ol => ol.ToOccupationalLevelDto())
      .ToList();
    }

    public async Task<OccupationalLevelDto?> GetOccupationalLevelByIdAsync(int id)
    {
      var occupationalLevel = await _occupationalLevelRepo.GetOccupationalLevelByIdAsync(id);
      return occupationalLevel?.ToOccupationalLevelDto();
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
        CreatedDate = DateTime.UtcNow
      };

      await _occupationalLevelRepo.AddOccupationalLevelAsync(occupationalLevel);
      return occupationalLevel.ToOccupationalLevelDto();
    }

    public async Task<OccupationalLevelDto?> UpdateOccupationalLevelAsync(int id, UpdateOccupationalLevelDto updateOccupationalLevelDto)
    {
        ArgumentNullException.ThrowIfNull(updateOccupationalLevelDto);

      var occupationalLevel = await _occupationalLevelRepo.GetOccupationalLevelByIdAsync(id); 
        if (occupationalLevel == null) return null;
    
     var trimmedDescription = updateOccupationalLevelDto.Description.Trim();

      // Prevent duplicate
      if (!string.Equals(trimmedDescription, occupationalLevel.Description, StringComparison.OrdinalIgnoreCase))
      {
          var existing = await _occupationalLevelRepo.GetOccupationalLevelByDescriptionAsync(trimmedDescription);
          if (existing != null && existing.OccupationalLevelId != id)
              throw new InvalidOperationException("Another occupational level with this description already exists.");
      }
      occupationalLevel.Description = trimmedDescription;
      occupationalLevel.CreatedDate = DateTime.UtcNow;

      await _occupationalLevelRepo.UpdateOccupationalLevelAsync(occupationalLevel);
      return occupationalLevel.ToOccupationalLevelDto();
    }

}
}

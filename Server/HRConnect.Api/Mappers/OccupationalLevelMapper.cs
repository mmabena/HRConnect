namespace HRConnect.Api.Mappers
{
  using System.Collections.Generic;
  using HRConnect.Api.DTOs.OccupationalLevel;
  using HRConnect.Api.Models;

  public static class OccupationalLevelMapper
  {
    public static OccupationalLevelDto ToOccupationalLevelDto(this CreateOccupationalLevelDto createoccupationalLevelDto)
    {
      return new OccupationalLevelDto
      {
         Description = createoccupationalLevelDto.Description,
         CreatedDate = DateTime.UtcNow
      };        
    }

    public static OccupationalLevelDto ToOccupationalLevelDto(this OccupationalLevel occupationalLevel)
    {
      return new OccupationalLevelDto
      {
        OccupationalLevelId = occupationalLevel.OccupationalLevelId,
        Description = occupationalLevel.Description,
        CreatedDate = occupationalLevel.CreatedDate,
        UpdatedDate = occupationalLevel.UpdatedDate,
      };
    }
  }
}
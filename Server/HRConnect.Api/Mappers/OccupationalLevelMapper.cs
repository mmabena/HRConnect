namespace HRConnect.Api.Mappers
{
  using System.Collections.Generic;
  using HRConnect.Api.DTOs.OccupationalLevel;
  using HRConnect.Api.Models;

  public static class OccupationalLevelMapper
  {
    public static OccupationalLevelDto ToOccupationalLevelDto(this OccupationalLevel occupationalLevel)
    {
      return new OccupationalLevelDto
      {
        Description = occupationalLevel.Description
      };
    }
  }
}
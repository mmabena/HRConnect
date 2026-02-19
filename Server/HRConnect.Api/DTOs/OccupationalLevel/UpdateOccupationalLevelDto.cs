namespace HRConnect.Api.DTOs.OccupationalLevel
{
  using System;
  public class UpdateOccupationalLevelDto
  {
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
  }
}
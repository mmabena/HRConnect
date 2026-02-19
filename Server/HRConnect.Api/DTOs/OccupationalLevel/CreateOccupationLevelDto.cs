namespace HRConnect.Api.DTOs.OccupationalLevel
{
  using System;
  public class CreateOccupationalLevelDto
{ 
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
  }
}
namespace HRConnect.Api.DTOs.OccupationalLevel
{
  using System;
  public class CreateOccupationalLevelDto
{

  public int OccupationalLevelId { get; set; }  
    public string Description { get; set; } = string.Empty;
  }
}
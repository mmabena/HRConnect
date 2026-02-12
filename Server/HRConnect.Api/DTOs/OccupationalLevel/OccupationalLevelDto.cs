namespace HRConnect.Api.DTOs.OccupationalLevel
{
  using System.Collections.Generic;
  using HRConnect.Api.DTOs.Position;

  public class OccupationalLevelDto
  {
      public int OccupationalLevelId { get; set; }
       public string Description { get; set; } = string.Empty;
      public DateTime CreatedDate { get; set; }
      public DateTime UpdatedDate { get; set; }
    
  }
}
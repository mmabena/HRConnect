namespace HRConnect.Api.DTOs.OccupationalLevel
{   using System;
   using HRConnect.Api.DTOs.JobGrade;
   using HRConnect.Api.DTOs.Position;
  public class OccupationalLevelDto
  {
      public int OccupationalLevelId { get; set; }
       public string Description { get; set; } = string.Empty;
      public bool IsActive { get; set; } = true;
      public DateTime CreatedDate { get; set; }
      public DateTime UpdatedDate { get; set; }
    
  }
}
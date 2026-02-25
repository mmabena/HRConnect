namespace HRConnect.Api.DTOs.JobGrade
{
  using System.Collections.Generic;
  using HRConnect.Api.DTOs.Position;
  public class JobGradeDto
  {
    public int JobGradeId { get; set; }   
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
     public DateTime CreatedDate { get; set; }
     public DateTime UpdatedDate { get; set; }

  }
}
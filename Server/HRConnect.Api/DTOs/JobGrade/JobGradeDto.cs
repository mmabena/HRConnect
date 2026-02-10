namespace HRConnect.Api.DTOs.JobGrade
{
  using System.Collections.Generic;
  using HRConnect.Api.DTOs.Position;
  public class JobGradeDto
  {
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }

  }
}
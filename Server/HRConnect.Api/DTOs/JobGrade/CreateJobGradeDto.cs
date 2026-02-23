namespace HRConnect.Api.DTOs.JobGrade
{
  using System;
  public class CreateJobGradeDto
  {
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
  }
}
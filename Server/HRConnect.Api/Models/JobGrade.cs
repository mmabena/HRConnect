namespace HRConnect.Api.Models
{
  using System.Collections.Generic;

  public class JobGrade
  {
    public int JobGradeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    public ICollection<Position> Positions { get; set; }
  }
}
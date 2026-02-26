namespace HRConnect.Api.Models
{

  public class Position
  {
    public int PositionId { get; set; }
    public string PositionTitle { get; set; } = string.Empty;
    public int JobGradeId { get; set; }
    public int OccupationalLevelId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; }
  }
}
namespace HRConnect.Api.DTOs.Position

{
  public class UpdatePositionDto
  {
    public string Title { get; set; } = string.Empty;
    public int JobGradeId { get; set; }
    public int OccupationalLevelId { get; set; }
    public bool IsActive { get; set; }
  }
}
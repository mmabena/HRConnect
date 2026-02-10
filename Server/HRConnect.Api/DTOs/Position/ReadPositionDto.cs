namespace HRConnect.Api.DTOs.Position
{
  public class ReadPositionDto
  {
    public int PositionId  { get; set; }
    public string Title { get; set; }
    public int JobGradeId { get; set; }
    public int OccupationalLevelId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; }

    public JobGradeDto JobGrade { get; set; }
    public OccupationalLevelDto OccupationalLevel { get; set; }
  }
}
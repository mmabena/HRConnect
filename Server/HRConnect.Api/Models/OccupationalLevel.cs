namespace HRConnect.Api.Models
{
  using System.Collections.Generic;

  public class OccupationalLevel
  {
    public int OccupationalLevelId { get; set; }
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    public ICollection<Position> Positions { get; set; }
  }
}
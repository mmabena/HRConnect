namespace HRConnect.Api.Models
{
  using System.Collections.Generic;

  public class OccupationalLevel
  {
    public int OccupationalLevelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public ICollection<Position> Positions { get; set; }
  }
}
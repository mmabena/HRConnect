namespace HRConnect.Api.Models
{
  public class JobGrade
  {
    public int JobGradeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    public ICollection<Position> Positions { get; set; } = new List<Position>();

    public ICollection<LeaveEntitlementRule> LeaveEntitlementRules { get; set; }
        = new List<LeaveEntitlementRule>();
  }
}
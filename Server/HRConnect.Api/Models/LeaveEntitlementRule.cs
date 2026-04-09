namespace HRConnect.Api.Models
{
  using System.Collections.Generic;
  public class LeaveEntitlementRule
  {
    public int Id { get; set; }

    public int LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;

    public int? JobGradeId { get; set; }
    public JobGrade? JobGrade { get; set; }

    public decimal MinYearsService { get; set; }
    public decimal? MaxYearsService { get; set; }

    public decimal DaysAllocated { get; set; }

    public bool IsActive { get; set; }

  }
}
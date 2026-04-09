namespace HRConnect.Api.DTOs
{
  using System.Collections.Generic;

  public class CreateLeaveTypeRequestDto
  {
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public bool FemaleOnly { get; set; }
    public List<LeaveEntitlementRuleRequestDto> Rules { get; set; } = new();
  }

  public class LeaveEntitlementRuleRequestDto
  {
    public int JobGradeId { get; set; }
    public decimal MinYearsService { get; set; }
    public decimal? MaxYearsService { get; set; }
    public decimal DaysAllocated { get; set; }
  }

  public class UpdateLeaveTypeRequestDto
  {
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool FemaleOnly { get; set; }
    public List<LeaveEntitlementRuleRequestDto> Rules { get; set; } = new();
  }

  public class LeaveTypeResponseDto
  {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public bool FemaleOnly { get; set; }
    public bool IsActive { get; set; }
    public List<LeaveEntitlementRuleSummaryDto> Rules { get; set; } = new();
  }

  public class LeaveEntitlementRuleSummaryDto
  {
    public int JobGradeId { get; set; }
    public decimal MinYearsService { get; set; }
    public decimal? MaxYearsService { get; set; }
    public decimal DaysAllocated { get; set; }
  }
}
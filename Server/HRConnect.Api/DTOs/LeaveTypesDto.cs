namespace HRConnect.Api.DTOs
{
  using System.Collections.Generic;

  public class CreateLeaveTypeRequest
  {
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public bool FemaleOnly { get; set; }
    public List<LeaveEntitlementRuleRequest> Rules { get; set; } = new();
  }

  public class LeaveEntitlementRuleRequest
  {
    public string GroupKey { get; set; } = null!;
    public decimal MinYearsService { get; set; }
    public decimal? MaxYearsService { get; set; }
    public decimal DaysAllocated { get; set; }
  }

  public class UpdateLeaveTypeRequest
  {
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool FemaleOnly { get; set; }
    public bool IsActive { get; set; }
    public List<LeaveEntitlementRuleRequest> Rules { get; set; } = new();
  }

  public class LeaveTypeResponse
  {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public bool FemaleOnly { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<LeaveEntitlementRuleSummary> Rules { get; set; } = new();
  }

  public class LeaveEntitlementRuleSummary
  {
    public string GroupKey { get; set; } = null!;
    public decimal MinYearsService { get; set; }
    public decimal? MaxYearsService { get; set; }
    public decimal DaysAllocated { get; set; }
  }

  public class EntitlementImpactPreviewDto
  {
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string GroupKey { get; set; } = string.Empty;
    public decimal YearsOfService { get; set; }
    public decimal PreviousEntitlement { get; set; } 
    public decimal NewEntitlement { get; set; } 
  }
}
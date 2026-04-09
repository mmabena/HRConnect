namespace HRConnect.Api.Mappers
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Models;

  public static class LeaveMapper
  {
    public static LeaveTypeResponseDto ToLeaveTypeResponseDto(this LeaveType? leaveType)
    {
      return new LeaveTypeResponseDto
      {
        Id = leaveType.Id,
        Name = leaveType.Name,
        Code = leaveType.Code,
        FemaleOnly = leaveType.FemaleOnly,
        IsActive = leaveType.IsActive,
        Rules = leaveType.EntitlementRules.Select(r => new LeaveEntitlementRuleSummaryDto
        {
          JobGradeId = r.JobGradeId,
          MinYearsService = r.MinYearsService,
          MaxYearsService = r.MaxYearsService,
          DaysAllocated = r.DaysAllocated
        }).ToList()
      };
    }

    public static EmployeeWithLeaveDto ToEmployeeWithLeaveDto(this Employee e)
    {
      return new EmployeeWithLeaveDto
      {
        EmployeeId = e.EmployeeId,
        FullName = e.Name + " " + e.Surname,
        Email = e.Email,
        Position = e.Position!.PositionTitle,
        LeaveBalances = e.LeaveBalances.Select(lb => new LeaveBalanceSummary
        {
          LeaveType = lb.LeaveType.Name,
          AccruedDays = lb.AccruedDays,
          TakenDays = lb.TakenDays,
          AvailableDays = lb.AvailableDays
        }).ToList()
      };
    }
  }
}
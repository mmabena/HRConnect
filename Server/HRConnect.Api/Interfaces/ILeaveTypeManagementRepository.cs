namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Models;

  public interface ILeaveTypeManagementRepository
  {
    Task<List<LeaveTypeResponseDto>> GetLeaveTypesAsync();
    Task<List<EmployeeWithLeaveDto>> GetAllEmployeesWithLeaveAsync();
    Task<EmployeeWithLeaveDto?> GetEmployeeWithLeaveByIdAsync(string employeeId);
    Task<LeaveTypeResponseDto?> GetLeaveTypeByIdAsync(int id);
    Task<List<string>?> GetExistingNames(string name);
    Task<List<string>?> GetExistingCodes(string code);
    Task<LeaveType> CreateLeaveTypeAsync(LeaveType leaveType);
    Task<List<LeaveEntitlementRule>> CreateLeaveEntitlementRules(List<LeaveEntitlementRule> rules);
  }
}
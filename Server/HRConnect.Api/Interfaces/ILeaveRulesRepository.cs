namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;

  public interface ILeaveRulesRepository
  {
    Task<LeaveEntitlementRule?> GetLeaveEntitlementRuleByIdAsync(int id);
    Task<Employee?> GetEmployeeByJobGradeIdAsync(int id);
    Task<EmployeeAccrualRateHistory?> GetEmployeeAccrualRateHistoryAsync(string employeeId);
  }
}
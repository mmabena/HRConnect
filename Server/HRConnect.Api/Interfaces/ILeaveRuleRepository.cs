namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;

  public interface ILeaveRuleRepository
  {
    Task<LeaveEntitlementRule?> GetLeaveEntitlementRuleByIdAsync(int ruleId);
    Task<List<Employee>> GetEmployeeByJobGradeIdAsync(int jobGradeId);
    Task<List<EmployeeAccrualRateHistory>> GetEmployeeAccrualRateHistoryAsync(List<string> employeeIds);
  }
}
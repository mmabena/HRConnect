namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using Microsoft.EntityFrameworkCore;
  using HRConnect.Api.Models;
  public class LeaveRuleRepository : ILeaveRuleRepository
  {
    private readonly ApplicationDBContext _context;
    public LeaveRuleRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task<LeaveEntitlementRule?> GetLeaveEntitlementRuleByIdAsync(int ruleId)
    {
      return await _context.LeaveEntitlementRules
                            .Include(r => r.LeaveType)
                            .FirstOrDefaultAsync(r => r.Id == ruleId);

    }
    public async Task<List<Employee>> GetEmployeeByJobGradeIdAsync(int jobGradeId)
    {
      return await _context.Employees
                           .Include(e => e.Position)
                           .Include(e => e.LeaveBalances)
                           .Where(e =>
                               (new[] { 2, 3, 4, 6 }.Contains(e.Position!.JobGradeId) &&
                                new[] { 2, 3, 4, 6 }.Contains(jobGradeId))
                               ||
                               e.Position.JobGradeId == jobGradeId)
                           .ToListAsync();

    }
    public async Task<List<EmployeeAccrualRateHistory>> GetEmployeeAccrualRateHistoryAsync(List<string> employeeIds)
    {
      return await _context.EmployeeAccrualRateHistories
                            .Where(x => employeeIds.Contains(x.EmployeeId) && x.EffectiveTo == null)
                            .ToListAsync();
    }
  }
}
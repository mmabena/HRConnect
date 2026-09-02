namespace HRConnect.Api.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;
    public class EmployeeLeaveBalanceRepository : IEmployeeLeaveBalanceRepository
    {
        private readonly ApplicationDBContext _context;

        public EmployeeLeaveBalanceRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetEmployeeWithLeaveBalancesAsync(string employeeId)
        {
            return await _context.Employees
                .Include(e => e.Position)
                    .ThenInclude(p => p.JobGrade)
                .Include(e => e.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<string?> GetGroupKeyByJobGradeIdAsync(int jobGradeId)
        {
            return await _context.JobGradeGroupMaps
                .Where(x => x.JobGradeId == jobGradeId)
                .Select(x => x.GroupKey)
                .FirstOrDefaultAsync();
        }

        public async Task<List<LeaveType>> GetActiveLeaveTypesAsync()
        {
            return await _context.LeaveTypes
                .Where(l => l.IsActive)
                .ToListAsync();
        }

        public async Task<LeaveEntitlementRule?> GetApplicableLeaveRuleAsync(
            int leaveTypeId,
            string groupKey,
            decimal yearsOfService)
        {
            return await _context.LeaveEntitlementRules
                .Where(r =>
                    r.LeaveTypeId == leaveTypeId &&
                    r.GroupKey == groupKey &&
                    r.MinYearsService <= yearsOfService &&
                    (r.MaxYearsService == null ||
                     yearsOfService < r.MaxYearsService) &&
                    r.IsActive)
                .OrderByDescending(r => r.MinYearsService)
                .FirstOrDefaultAsync();
        }

        public async Task AddLeaveBalanceAsync(EmployeeLeaveBalance balance)
        {
            await _context.EmployeeLeaveBalances.AddAsync(balance);
        }

        public async Task AddLeaveBalancesAsync(
            List<EmployeeLeaveBalance> balances)
        {
            await _context.EmployeeLeaveBalances.AddRangeAsync(balances);
        }

        public async Task<bool> HasAccrualRateHistoryAsync(string employeeId)
        {
            return await _context.EmployeeAccrualRateHistories
                .AnyAsync(s => s.EmployeeId == employeeId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<EmployeeLeaveBalance?> GetLeaveBalanceAsync(
            string employeeId,
            int leaveTypeId)
        {
            return await _context.EmployeeLeaveBalances
                .Include(b => b.LeaveType)
                    .ThenInclude(lt => lt.EntitlementRules)
                .Include(b => b.Employee)
                .FirstOrDefaultAsync(b =>
                    b.EmployeeId == employeeId &&
                    b.LeaveTypeId == leaveTypeId);
        }

        public async Task<Employee?> GetEmployeeForAnnualLeaveAsync(string employeeId)
        {
            return await _context.Employees
                .Include(e => e.LeaveBalances)
                .Include(e => e.Position)
                    .ThenInclude(p => p.JobGrade)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<LeaveType?> GetActiveAnnualLeaveTypeAsync()
        {
            return await _context.LeaveTypes
                .FirstOrDefaultAsync(l =>
                    l.Code == "AL" &&
                    l.IsActive);
        }

        public async Task<List<EmployeeAccrualRateHistory>> GetEmployeeAccrualRateHistoriesAsync(string employeeId)
        {
            return await _context.EmployeeAccrualRateHistories
                .Where(x => x.EmployeeId == employeeId)
                .OrderBy(x => x.EffectiveFrom)
                .ToListAsync();
        }

        public async Task<List<LeaveEntitlementRule>> GetAnnualLeaveRulesAsync(int leaveTypeId, string groupKey)
        {
            return await _context.LeaveEntitlementRules
                .Where(r =>
                    r.LeaveTypeId == leaveTypeId &&
                    r.GroupKey == groupKey &&
                    r.IsActive)
                .OrderBy(r => r.MinYearsService)
                .ToListAsync();
        }

        public async Task<EmployeeLeaveBalance?> GetEmployeeLeaveBalanceAsync(string employeeId, int leaveTypeId)
        {
            return await _context.EmployeeLeaveBalances
                .FirstOrDefaultAsync(b =>
                    b.EmployeeId == employeeId &&
                    b.LeaveTypeId == leaveTypeId);
        }

        public async Task<LeaveEntitlementRule?> GetHistoricalAnnualLeaveRuleAsync(int leaveTypeId, string groupKey, decimal yearsOfService)
        {
            return await _context.LeaveEntitlementRules
                .Where(r =>
                    r.LeaveTypeId == leaveTypeId &&
                    r.GroupKey == groupKey &&
                    r.MinYearsService <= yearsOfService &&
                    (r.MaxYearsService == null ||
                     yearsOfService < r.MaxYearsService) &&
                    r.IsActive)
                .OrderByDescending(r => r.MinYearsService)
                .FirstOrDefaultAsync();
        }

        public async Task<EmployeeAccrualRateHistory?> GetCurrentAccrualSegmentAsync(string employeeId)
        {
            return await _context.EmployeeAccrualRateHistories
                .Where(x =>
                    x.EmployeeId == employeeId &&
                    x.EffectiveTo == null)
                .OrderByDescending(x => x.EffectiveFrom)
                .FirstOrDefaultAsync();
        }

        public async Task AddAccrualRateHistoryAsync(EmployeeAccrualRateHistory history)
        {
            await _context.EmployeeAccrualRateHistories.AddAsync(history);
        }

        public void RemoveAccrualRateHistory(EmployeeAccrualRateHistory history)
        {
            _context.EmployeeAccrualRateHistories.Remove(history);
        }

    }
}
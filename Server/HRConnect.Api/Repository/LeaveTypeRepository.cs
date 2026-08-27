namespace HRConnect.Api.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HRConnect.Api.Data;
    using HRConnect.Api.Models;
    using HRConnect.Api.Interfaces;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    public class LeaveTypeRepository : ILeaveTypeRepository
    {
        private readonly ApplicationDBContext _context;

        public LeaveTypeRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<LeaveType>> GetAllLeaveTypesAsync()
        {
            return await _context.LeaveTypes
                            .Include(l => l.EntitlementRules)
                            .ToListAsync();
        }
        public async Task<LeaveType?> GetLeaveTypeWithRulesAsync(int leaveTypeId)
        {
            return await _context.LeaveTypes
                .Include(l => l.EntitlementRules)
                .FirstOrDefaultAsync(l => l.Id == leaveTypeId);
        }

        public async Task<List<string>> GetLeaveTypeNamesAsync()
        {
            return await _context.LeaveTypes
                .Select(l => l.Name)
                .ToListAsync();
        }

        public async Task<List<string>> GetLeaveTypeCodesAsync()
        {
            return await _context.LeaveTypes
                .Select(l => l.Code)
                .ToListAsync();
        }

        public async Task<List<string>> GetValidGroupKeysAsync()
        {
            return await _context.JobGradeGroupMaps
                .Select(x => x.GroupKey)
                .Distinct()
                .ToListAsync();
        }

        public async Task<LeaveType> CreateLeaveTypeAsync(LeaveType leaveType)
        {
            await _context.LeaveTypes.AddAsync(leaveType);
            await _context.SaveChangesAsync();

            return leaveType;
        }

        public async Task AddLeaveEntitlementRulesAsync(List<LeaveEntitlementRule> rules)
        {
            await _context.LeaveEntitlementRules
                .AddRangeAsync(rules);

            await _context.SaveChangesAsync();
        }

        public async Task<LeaveType?> GetLeaveTypeWithRulesForUpdateAsync(int leaveTypeId)
        {
            return await _context.LeaveTypes
                .Include(l => l.EntitlementRules)
                .FirstOrDefaultAsync(l => l.Id == leaveTypeId);
        }

        public async Task DeleteEntitlementRulesAsync(ICollection<LeaveEntitlementRule> rules)
        {
            _context.LeaveEntitlementRules.RemoveRange(rules);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LeaveType>> GetActiveLeaveTypesAsync()
        {
            return await _context.LeaveTypes
                .Where(l => l.IsActive)
                .ToListAsync();
        }

        public async Task<LeaveEntitlementRule?> GetApplicableEntitlementRuleAsync(int leaveTypeId, string groupKey, decimal yearsOfService)
        {
            return await _context.LeaveEntitlementRules
                .Where(r =>
                    r.LeaveTypeId == leaveTypeId &&
                    r.GroupKey == groupKey &&
                    r.MinYearsService <= yearsOfService &&
                    (
                        r.MaxYearsService == null ||
                        yearsOfService < r.MaxYearsService
                    ) &&
                    r.IsActive)
                .OrderByDescending(r => r.MinYearsService)
                .FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetLeaveTypeNamesExceptAsync(int leaveTypeId)
        {
            return await _context.LeaveTypes
                .Where(x => x.Id != leaveTypeId)
                .Select(x => x.Name)
                .ToListAsync();
        }

        public async Task<LeaveType?> GetActiveLeaveTypeByCodeAsync(string code)
        {
            return await _context.LeaveTypes
                .FirstOrDefaultAsync(l =>
                    l.Code == code &&
                    l.IsActive);
        }

        public async Task<string?> GetGroupKeyByJobGradeIdAsync(int jobGradeId)
        {
            return await _context.JobGradeGroupMaps
                .Where(x => x.JobGradeId == jobGradeId)
                .Select(x => x.GroupKey)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateLeaveTypeWithRulesAsync(LeaveType leaveType, ICollection<LeaveEntitlementRule> existingRules, List<LeaveEntitlementRule> newRules)
        {
            _context.LeaveEntitlementRules.RemoveRange(existingRules);

            await _context.LeaveEntitlementRules.AddRangeAsync(newRules);

            _context.LeaveTypes.Update(leaveType);

            await _context.SaveChangesAsync();
        }

        public async Task<LeaveEntitlementRule?> GetLeaveRuleWithLeaveTypeAsync(int ruleId)
        {
            return await _context.LeaveEntitlementRules
                .Include(r => r.LeaveType)
                .FirstOrDefaultAsync(r => r.Id == ruleId);
        }

        public async Task<List<Employee>> GetEmployeesForLeaveRuleAsync(string groupKey)
        {
            return await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.LeaveBalances)
                .Where(e =>
                    groupKey == "ALL" ||
                    _context.JobGradeGroupMaps
                        .Where(m => m.GroupKey == groupKey)
                        .Select(m => m.JobGradeId)
                        .Contains(e.Position.JobGradeId))
                .ToListAsync();
        }

        public async Task<List<EmployeeAccrualRateHistory>> GetActiveAccrualRateHistoriesAsync(
    List<string> employeeIds)
        {
            return await _context.EmployeeAccrualRateHistories
                .Where(x =>
                    employeeIds.Contains(x.EmployeeId) &&
                    x.EffectiveTo == null)
                .ToListAsync();
        }

        public async Task UpdateLeaveRuleAsync(LeaveEntitlementRule rule)
        {
            _context.LeaveEntitlementRules.Update(rule);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<List<EmployeeLeaveBalance>> GetAnnualLeaveBalancesAsync(int leaveTypeId)
        {
            return await _context.EmployeeLeaveBalances
                .Include(b => b.Employee)
                .Where(b => b.LeaveTypeId == leaveTypeId)
                .ToListAsync();
        }

        public async Task<bool> AnnualLeaveHistoryExistsAsync(string employeeId, int year)
        {
            return await _context.AnnualLeaveAccrualHistories
                .AnyAsync(x =>
                    x.EmployeeId == employeeId &&
                    x.Year == year);
        }
        public async Task AddAnnualLeaveAccrualHistoryAsync(AnnualLeaveAccrualHistory history)
        {
            await _context.AnnualLeaveAccrualHistories
                .AddAsync(history);
        }
        public async Task<List<LeaveApplication>> GetExpiredPendingLeaveApplicationsAsync(DateTime expiryCutoff)
        {
            return await _context.LeaveApplications
                .Include(a => a.Documents)
                .Where(a =>
                    a.Status ==
                        LeaveApplication.LeaveApplicationStatus.Pending &&
                    a.AppliedDate <= expiryCutoff)
                .ToListAsync();
        }
        public async Task<LeaveType?> GetLeaveTypeByIdAsync(int leaveTypeId)
        {
            return await _context.LeaveTypes
                .FirstOrDefaultAsync(l =>
                    l.Id == leaveTypeId);
        }



    }
}
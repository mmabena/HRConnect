namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HRConnect.Api.Models;
    using System.Threading.Tasks;
    public interface ILeaveTypeRepository
    {
        Task<List<LeaveType>> GetAllLeaveTypesAsync();
        Task<LeaveType?> GetLeaveTypeWithRulesAsync(int leaveTypeId);
        Task<List<string>> GetLeaveTypeNamesAsync();
        Task<List<string>> GetLeaveTypeCodesAsync();
        Task<List<string>> GetValidGroupKeysAsync();
        Task<LeaveType> CreateLeaveTypeAsync(
            LeaveType leaveType);
        Task AddLeaveEntitlementRulesAsync(
            List<LeaveEntitlementRule> rules);
        Task<LeaveType?> GetLeaveTypeWithRulesForUpdateAsync(int leaveTypeId);
        Task DeleteEntitlementRulesAsync(
            ICollection<LeaveEntitlementRule> rules);
        Task<List<LeaveType>> GetActiveLeaveTypesAsync();
        Task<LeaveEntitlementRule?> GetApplicableEntitlementRuleAsync(
            int leaveTypeId,
            string groupKey,
            decimal yearsOfService);


        Task<List<string>> GetLeaveTypeNamesExceptAsync(int leaveTypeId);

        Task UpdateLeaveTypeWithRulesAsync(
            LeaveType leaveType,
            ICollection<LeaveEntitlementRule> existingRules,
            List<LeaveEntitlementRule> newRules);

        Task<LeaveType?> GetActiveLeaveTypeByCodeAsync(string code);

        Task<string?> GetGroupKeyByJobGradeIdAsync(int jobGradeId);

        Task<LeaveEntitlementRule?> GetLeaveRuleWithLeaveTypeAsync(int ruleId);

        Task<List<Employee>> GetEmployeesForLeaveRuleAsync(string groupKey);
        Task<List<EmployeeAccrualRateHistory>> GetActiveAccrualRateHistoriesAsync( List<string> employeeIds);
        Task UpdateLeaveRuleAsync(LeaveEntitlementRule rule);
        Task SaveChangesAsync();
        Task<List<EmployeeLeaveBalance>> GetAnnualLeaveBalancesAsync(int leaveTypeId);
        Task<bool> AnnualLeaveHistoryExistsAsync(string employeeId, int year);
        Task AddAnnualLeaveAccrualHistoryAsync(AnnualLeaveAccrualHistory history);
        Task<List<LeaveApplication>> GetExpiredPendingLeaveApplicationsAsync(DateTime expiryCutoff);
        Task<LeaveType?> GetLeaveTypeByIdAsync(int leaveTypeId);
    }
}
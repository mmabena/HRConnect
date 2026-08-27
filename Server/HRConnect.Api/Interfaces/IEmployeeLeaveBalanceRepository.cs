namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;
    public interface IEmployeeLeaveBalanceRepository
    {
        Task<Employee?> GetEmployeeWithLeaveBalancesAsync(string employeeId);

        Task<string?> GetGroupKeyByJobGradeIdAsync(int jobGradeId);

        Task<List<LeaveType>> GetActiveLeaveTypesAsync();

        Task<LeaveEntitlementRule?> GetApplicableLeaveRuleAsync(
            int leaveTypeId,
            string groupKey,
            decimal yearsOfService);

        Task AddLeaveBalanceAsync(EmployeeLeaveBalance balance);

        Task AddLeaveBalancesAsync(List<EmployeeLeaveBalance> balances);

        Task<bool> HasAccrualRateHistoryAsync(string employeeId);

        Task SaveChangesAsync();

        Task<EmployeeLeaveBalance?> GetLeaveBalanceAsync(
            string employeeId,
            int leaveTypeId);

        Task<Employee?> GetEmployeeForAnnualLeaveAsync(string employeeId);

        Task<LeaveType?> GetActiveAnnualLeaveTypeAsync();

        Task<List<EmployeeAccrualRateHistory>> GetEmployeeAccrualRateHistoriesAsync(
            string employeeId);

        Task<List<LeaveEntitlementRule>> GetAnnualLeaveRulesAsync(int leaveTypeId, string groupKey);
        Task<EmployeeLeaveBalance?> GetEmployeeLeaveBalanceAsync(string employeeId, int leaveTypeId);
        Task<LeaveEntitlementRule?> GetHistoricalAnnualLeaveRuleAsync(int leaveTypeId, string groupKey, decimal yearsOfService);
        Task<EmployeeAccrualRateHistory?> GetCurrentAccrualSegmentAsync(string employeeId);
        Task AddAccrualRateHistoryAsync(EmployeeAccrualRateHistory history);
        void RemoveAccrualRateHistory(EmployeeAccrualRateHistory history);

    }
}
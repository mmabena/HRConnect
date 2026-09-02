namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using HRConnect.Api.DTOs;
    using System.Collections.Generic;
    using HRConnect.Api.Models;
    public interface ILeaveBalanceService
    {
        Task InitializeEmployeeLeaveBalancesAsync(string employeeId);
        Task UpdateTakenDaysAsync(UpdateTakenDaysRequest request);
        Task RecalculateAnnualLeaveAsync(string employeeId);
        Task RecalculateSickLeaveAsync(string employeeId);
        Task RecalculateFamilyResponsibilityLeaveAsync(string employeeId);
        Task ResetMaternityLeaveForNewPregnancy(string employeeId);
        Task<LeaveProjectionResponse> ProjectAnnualLeaveAsync(string employeeId, DateOnly projectionDate);
        Task RecalculateFamilyResponsibilityLeaveBulkAsync(List<string> employeeIds);
        Task RecalculateAnnualLeaveBulkAsync(List<string> employeeIds);
        Task CreateAccrualSegmentAsync(
                Employee employee,
                decimal annualEntitlement,
                string reason,
                DateOnly effectiveFrom);
        Task CheckYearsOfServiceAccrualChangeAsync(
                string employeeId);
        Task ApplyEntitlementRuleChangesAsync();
    }
}
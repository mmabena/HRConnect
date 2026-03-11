namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    using HRConnect.Api.DTOs;

    public interface IEmployeeEntitlementService
    {
        Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request);
        Task<List<EmployeeResponse>> GetAllEmployeesAsync();
        Task<EmployeeResponse?> GetEmployeeByIdAsync(string id);
        Task<EmployeeResponse> UpdateEmployeePositionAsync(string employeeId, int newPositionId);
        Task InitializeEmployeeLeaveBalancesAsync(string employeeId);
        Task RecalculateAnnualLeaveAsync(string employeeId);
        Task RecalculateSickLeaveAsync(string employeeId);
        Task RecalculateAllSickLeaveAsync();
        Task UpdateTakenDaysAsync(UpdateTakenDaysRequest request);
        Task ResetMaternityLeaveForNewPregnancy(string employeeId);
        Task ProcessCarryOverNotificationAsync();
        Task UpdateLeaveEntitlementRuleAsync(UpdateLeaveRuleRequest request);
        Task RecalculateEmployeesForRuleChangeAsync(LeaveEntitlementRule rule);
        Task RecalculateAllFamilyResponsibilityLeaveAsync();
        Task RecalculateFamilyResponsibilityLeaveAsync(string employeeId);
        Task<LeaveProjectionResponse> ProjectAnnualLeaveAsync(string employeeId, DateOnly projectionDate);
        Task ProcessAnnualResetAsync(int? overrideYear = null);
    }
}
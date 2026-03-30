namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using HRConnect.Api.DTOs;
    public interface ILeaveBalanceService
    {
        Task InitializeEmployeeLeaveBalancesAsync(string employeeId);
        Task UpdateTakenDaysAsync(UpdateTakenDaysRequest request);
        Task RecalculateAnnualLeaveAsync(string employeeId);
        Task RecalculateSickLeaveAsync(string employeeId);
        Task RecalculateFamilyResponsibilityLeaveAsync(string employeeId);
        Task ResetMaternityLeaveForNewPregnancy(string employeeId);
        Task<LeaveProjectionResponse> ProjectAnnualLeaveAsync(string employeeId, DateOnly projectionDate);
    }
}
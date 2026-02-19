
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
        Task<EmployeeResponse?> GetEmployeeByIdAsync(Guid id);
        Task<EmployeeResponse> UpdateEmployeePositionAsync(Guid employeeId, int newPositionId);
        Task DeleteEmployeeAsync(Guid id);
        Task InitializeEmployeeLeaveBalancesAsync(Guid employeeId);
        Task RecalculateAnnualLeaveAsync(Guid employeeId);
        Task ProcessCarryOverNotificationAsync();
        Task ProcessAnnualResetAsync();
        Task UpdateLeaveEntitlementRuleAsync(UpdateLeaveRuleRequest request);
        Task RecalculateEmployeesForRuleChangeAsync(LeaveEntitlementRule rule);
    }
}


namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    using HRConnect.Api.DTOs;


    public interface IEmployeeEntitlementService
    {
        // ============================
        // CREATE
        // ============================
        Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request);

        // ============================
        // READ
        // ============================
        Task<List<EmployeeResponse>> GetAllEmployeesAsync();

        Task<EmployeeResponse?> GetEmployeeByIdAsync(Guid id);

        // ============================
        // UPDATE
        // ============================
        Task<EmployeeResponse> UpdateEmployeePositionAsync(Guid employeeId, int newPositionId);

        // ============================
        // DELETE
        // ============================
        Task DeleteEmployeeAsync(Guid id);

        // ============================
        // INTERNAL LOGIC
        // ============================
        Task InitializeEmployeeLeaveBalancesAsync(Guid employeeId);

        Task RecalculateAnnualLeaveAsync(Guid employeeId);
    }
}

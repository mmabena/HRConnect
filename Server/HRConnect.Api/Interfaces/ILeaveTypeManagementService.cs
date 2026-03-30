namespace HRConnect.Api.Interfaces
{
    using HRConnect.Api.DTOs;

    public interface ILeaveTypeManagementService
    {
        Task<List<LeaveTypeResponse>> GetLeaveTypesAsync();

        Task<LeaveTypeResponse> GetLeaveTypeByIdAsync(int id);

        Task<LeaveTypeResponse> CreateLeaveTypeAsync(CreateLeaveTypeRequest request);

        Task<LeaveTypeResponse> UpdateLeaveTypeAsync(int id, UpdateLeaveTypeRequest request);
        Task<List<EmployeeWithLeaveDto>> GetAllEmployeesWithLeaveAsync();
        Task<EmployeeWithLeaveDto?> GetEmployeeWithLeaveByIdAsync(string employeeId);
    }
}
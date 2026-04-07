namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs;

  public interface ILeaveTypeManagementService
  {
    Task<List<LeaveTypeResponseDto>> GetLeaveTypesAsync();
    Task<LeaveTypeResponseDto> GetLeaveTypeByIdAsync(int id);
    Task<LeaveTypeResponseDto> CreateLeaveTypeAsync(CreateLeaveTypeRequestDto request);
    Task<LeaveTypeResponseDto> UpdateLeaveTypeAsync(int id, UpdateLeaveTypeRequestDto request);
    Task<List<EmployeeWithLeaveDto>> GetAllEmployeesWithLeaveAsync();
    Task<EmployeeWithLeaveDto?> GetEmployeeWithLeaveByIdAsync(string employeeId);
  }
}
namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.DTOs;

    public interface ILeaveApplicationService
    {

        Task<LeaveApplicationResponse> ApplyForLeaveAsync(CreateApplicationRequest request);
        Task ApproveLeaveAsync(int applicationId, Guid token);
        Task RejectLeaveAsync(int applicationId, Guid token, string? reason);
        Task ApproveLeaveInternalAsync(int applicationId);
        Task RejectLeaveInternalAsync(int applicationId, string? reason);
        Task<List<LeaveApplicationResponse>> GetAllAsync();
        Task<List<LeaveApplicationResponse>> GetByLeaveTypeCodeAsync(string code);
        Task<List<LeaveApplicationResponse>> GetByStatusAsync(string status);
        Task<List<LeaveApplicationResponse>> GetByEmployeeIdAsync(string employeeId);
        Task<List<LeaveApplicationResponse>> GetFilteredAsync(
    string? status,
    string? leaveTypeCode);

    }
}
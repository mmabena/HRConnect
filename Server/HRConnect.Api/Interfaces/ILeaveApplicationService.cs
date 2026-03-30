namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.DTOs;

    public interface ILeaveApplicationService
    {
        //  email flow
        Task<LeaveApplicationResponse> ApplyForLeaveAsync(CreateApplicationRequest request);
        Task ApproveLeaveAsync(int applicationId, Guid token);
        Task RejectLeaveAsync(int applicationId, Guid token, string? reason);
        // system flow
        Task ApproveLeaveInternalAsync(int applicationId);
        Task RejectLeaveInternalAsync(int applicationId, string? reason);
    }
}
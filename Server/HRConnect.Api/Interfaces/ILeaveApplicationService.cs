
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
        Task ApproveLeaveAsync(int applicationId, string token);
        Task RejectLeaveAsync(int applicationId, string token);
    }
}
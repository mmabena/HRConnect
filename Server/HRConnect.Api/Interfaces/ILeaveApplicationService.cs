namespace HRConnect.Api.Interfaces
{
  using System;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs;

  public interface ILeaveApplicationService
  {
    Task<LeaveApplicationResponse> ApplyForLeaveAsync(CreateApplicationRequest request);
    Task ApproveLeaveAsync(int applicationId, Guid token);
    Task RejectLeaveAsync(int applicationId, Guid token, string? reason);
    Task ApproveLeaveInternalAsync(int applicationId);
    Task RejectLeaveInternalAsync(int applicationId, string? reason);
  }
}
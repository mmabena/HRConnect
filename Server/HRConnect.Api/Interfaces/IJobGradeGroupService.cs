namespace HRConnect.Api.Interfaces
{
    using HRConnect.Api.DTOs;

    public interface IJobGradeGroupService
    {
        Task<List<JobGradeGroupResponse>> GetGroupsAsync();
        Task CreateGroupAsync(CreateGroupRequest request);
        Task MoveJobGradeAsync(MoveJobGradeRequest request);
        Task RemoveJobGradeAsync(RemoveJobGradeRequest request);
    }
}
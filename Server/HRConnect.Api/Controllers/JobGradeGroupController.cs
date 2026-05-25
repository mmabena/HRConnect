namespace HRConnect.Api.Controllers
{
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/job-grade-groups")]
    public class JobGradeGroupController : ControllerBase
    {
        private readonly IJobGradeGroupService _service;

        public JobGradeGroupController(
            IJobGradeGroupService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            var result = await _service.GetGroupsAsync();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup(
            CreateGroupRequest request)
        {
            await _service.CreateGroupAsync(request);

            return Ok("Group created successfully");
        }

        [HttpPut("move")]
        public async Task<IActionResult> MoveJobGrade(
            MoveJobGradeRequest request)
        {
            await _service.MoveJobGradeAsync(request);

            return Ok("JobGrade moved successfully");
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveJobGrade(
            RemoveJobGradeRequest request)
        {
            await _service.RemoveJobGradeAsync(request);

            return Ok("JobGrade removed successfully");
        }
    }
}
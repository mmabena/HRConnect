namespace HRConnect.Api.Controllers
{
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/leave-rules")]
    [Authorize(Roles = "SuperUser")]
    public class LeaveRuleController : ControllerBase
    {
        private readonly ILeaveRuleService _leaveRuleService;

        public LeaveRuleController(ILeaveRuleService leaveRuleService)
        {
            _leaveRuleService = leaveRuleService;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRule(
            int id,
            [FromBody] UpdateLeaveRuleRequest request)
        {
            request.RuleId = id;

            await _leaveRuleService.UpdateLeaveEntitlementRuleAsync(request);

            return Ok("Rule updated and employees recalculated.");
        }
    }
}
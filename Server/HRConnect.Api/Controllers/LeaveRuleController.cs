namespace HRConnect.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;

    [ApiController]
    [Route("api/leave-rules")]
    //[Authorize(Roles = "SuperUser")]
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
            // BASIC VALIDATION
            if (request == null)
                return BadRequest("Request body is required.");

            if (request.NewDaysAllocated < 0)
                return BadRequest("Days allocated cannot be negative.");

            if (id <= 0)
                return BadRequest("Invalid RuleId.");

            request.RuleId = id;

            try
            {
                await _leaveRuleService.UpdateLeaveEntitlementRuleAsync(request);

                return Ok(new
                {
                    Message = "Rule updated successfully.",
                    RuleId = id
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Rule not found.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
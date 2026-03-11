namespace HRConnect.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;

    [ApiController]
    [Route("api/[controller]")]
    public class LeaveApplicationController : ControllerBase
    {
        private readonly ILeaveApplicationService _service;

        public LeaveApplicationController(ILeaveApplicationService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> ApplyForLeave(CreateApplicationRequest request)
        {
            var result = await _service.ApplyForLeaveAsync(request);
            return Ok(result);
        }

        [HttpGet("{id}/approve")]
        public async Task<IActionResult> ApproveLeave(int id, [FromQuery] Guid token)
        {
            await _service.ApproveLeaveAsync(id, token);

            return Ok(new
            {
                message = "Leave approved successfully."
            });
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectLeave(
            int id,
            [FromQuery] Guid token,
            [FromBody] RejectLeaveRequest request)
        {
            await _service.RejectLeaveAsync(id, token, request.Reason);

            return Ok(new
            {
                message = "Leave rejected successfully."
            });
        }
    }
}

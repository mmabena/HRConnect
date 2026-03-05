using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HRConnect.Api.DTOs;
using HRConnect.Api.Interfaces;

namespace HRConnect.Api.Controllers
{
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
        public async Task<IActionResult> ApproveLeave(int id, string token)
        {
            await _service.ApproveLeaveAsync(id, token);

            return Content("""
    <html>
    <body style="font-family:Arial;text-align:center;margin-top:40px;">
        <h2 style="color:green;">Leave Approved</h2>
    </body>
    </html>
    """, "text/html");
        }

        [HttpGet("{id}/reject")]
        public async Task<IActionResult> RejectLeave(int id, string token)
        {
            await _service.RejectLeaveAsync(id, token);

            return Content("""
    <html>
    <body style="font-family:Arial;text-align:center;margin-top:40px;">
        <h2 style="color:red;">Leave Rejected</h2>
    </body>
    </html>
    """, "text/html");
        }
    }
}
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
    //[Authorize(Roles = "SuperUser")]
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

            return Content("""
    <html>
    <body style="font-family:Arial;text-align:center;margin-top:40px;">
        <h2 style="color:green;">Leave Approved</h2>
    </body>
    </html>
    """, "text/html");
        }

        [HttpGet("{id}/reject")]
        public IActionResult RejectLeave(int id, [FromQuery] Guid token)
        {
            return Content($"""
<html>
<body style="font-family:Arial;text-align:center;margin-top:40px;">

<h2 style="color:red;">Reject Leave Request</h2>

<form method="post" action="/api/LeaveApplication/{id}/reject?token={token}">
    
    <p>Please provide a reason for rejecting this leave request:</p>

    <textarea name="reason" rows="4" cols="40" required></textarea>

    <br><br>

    <button type="submit" 
        style="background-color:red;color:white;padding:10px 20px;border:none;">
        Confirm Rejection
    </button>

</form>

</body>
</html>
""", "text/html");
        }
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectLeavePost(
    int id,
    [FromQuery] Guid token,
    [FromForm] string reason)
        {
            await _service.RejectLeaveAsync(id, token, reason);

            return Content("""
<html>
<body style="font-family:Arial;text-align:center;margin-top:40px;">
    <h2 style="color:red;">Leave Rejected Successfully</h2>
</body>
</html>
""", "text/html");
        }
    }
}

namespace HRConnect.Api.Controllers
{
    using System;
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
        /// <summary>
        /// Handles the HTTP POST request to apply for leave, accepting a CreateApplicationRequest DTO in the request body,
        /// which contains the necessary information for the leave application, 
        /// and then calls the ApplyForLeaveAsync method of the ILeaveApplicationService to process the leave application,
        /// returning the result of the leave application process in the response body, 
        /// allowing employees to submit their leave requests through the API and receive feedback on the status of their applications.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ApplyForLeave([FromBody] CreateApplicationRequest request)
        {
            var result = await _service.ApplyForLeaveAsync(request);
            return Ok(result);
        }
        /// <summary>
        /// Handles the HTTP GET request to approve a leave application, 
        /// accepting the leave application ID as a route parameter and a token as a query parameter,
        /// which is used to verify the authenticity of the approval request.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveLeave(int id, [FromQuery] Guid token)
        {
            await _service.ApproveLeaveAsync(id, token);

            return Ok(new
            {
                message = "Leave approved successfully."
            });
        }
        /// <summary>
        /// Handles the HTTP POST request to reject a leave application,
        /// accepting the leave application ID as a route parameter, a token as a query parameter,
        /// and the rejection reason in the request body.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectLeave(
            int id,
            [FromQuery] Guid token)
        {
            await _service.RejectLeaveAsync(id, token, "Rejected by manager");

            return Ok(new
            {
                message = "Leave rejected successfully."
            });
        }
        [HttpGet("{id}/approve")]
        public async Task<IActionResult> ApproveFromEmail(
    int id,
    [FromQuery] Guid token)
        {
            // Internally call the POST logic
            await _service.ApproveLeaveAsync(id, token);

            return Content("Leave approved successfully.");
        }
        [HttpGet("{id}/reject")]
        public async Task<IActionResult> RejectFromEmail(
    int id,
    [FromQuery] Guid token)
        {
            await _service.RejectLeaveAsync(id, token, "Rejected by manager");

            return Content("Leave rejected successfully.");
        }
    }
}

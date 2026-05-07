namespace HRConnect.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Models;
    using HRConnect.Api.Interfaces;

    [ApiController]
    [Route("api/LeaveApplications")]
    public class LeaveApplicationsController : ControllerBase
    {
        private readonly ILeaveApplicationService _service;
        public LeaveApplicationsController(
    ILeaveApplicationService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> ApplyForLeave(
    [FromForm] CreateApplicationRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.EmployeeId))
                return BadRequest("Invalid request");
            try
            {
                var result = await _service.ApplyForLeaveAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _service.GetAllAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("by-employee-id/{employeeId}")]
        public async Task<IActionResult> GetByEmployeeId(string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                return BadRequest("EmployeeId is required.");

            try
            {
                var data = await _service.GetByEmployeeIdAsync(employeeId);

                return Ok(data);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}/approve")]
        public async Task<IActionResult> Approve(
     int id,
     [FromQuery] Guid token)
        {
            try
            {
                await _service.ApproveLeaveAsync(id, token);
                return Ok("Leave application approved successfully");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/reject")]
        public async Task<IActionResult> Reject(
      int id,
      [FromQuery] Guid token,
      [FromQuery] string? reason)
        {
            try
            {
                await _service.RejectLeaveAsync(id, token, reason);
                var applications = await _service.GetAllAsync();
                return Ok("Leave application rejected successfully");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/approve-admin")]
        public async Task<IActionResult> ApproveByAdmin(int id)
        {
            try
            {
                return Ok(new { message = "Approved successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPatch("{id}/reject-admin")]
        public async Task<IActionResult> RejectByAdmin(
            int id,
            [FromBody] DecisionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Reason))
                return BadRequest("Rejection reason is required");

            try
            {
                return Ok(new { message = "Rejected successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
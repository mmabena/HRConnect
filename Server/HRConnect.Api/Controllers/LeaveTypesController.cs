namespace HRConnect.Api.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Data;
    using HRConnect.Api.Models;
    using Microsoft.AspNetCore.Authorization;

    [ApiController]
    [Route("api/leave-types")]
    [Authorize(Roles = "SuperUser")]
    public class LeaveTypesController : ControllerBase
    {
        private readonly ILeaveTypeManagementService _service;
        private readonly ApplicationDBContext _context;

        public LeaveTypesController(
            ILeaveTypeManagementService service,
            ApplicationDBContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetLeaveTypesAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                return Ok(await _service.GetLeaveTypeByIdAsync(id));
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Leave type not found");
            }
        }

        [HttpGet("group-keys")]
        public async Task<IActionResult> GetGroupKeys()
        {
            var keys = await _context.JobGradeGroupMaps
                .Select(x => x.GroupKey)
                .Distinct()
                .ToListAsync();

            return Ok(keys);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLeaveTypeRequest request)
        {
            if (request == null || request.Rules == null || request.Rules.Count == 0)
                return BadRequest("At least one rule is required.");

            var validGroupKeys = await _context.JobGradeGroupMaps
                .Select(x => x.GroupKey)
                .Distinct()
                .ToListAsync();

            var invalidKeys = request.Rules
                .Where(r => r.GroupKey != "ALL" && !validGroupKeys.Contains(r.GroupKey))
                .Select(r => r.GroupKey)
                .Distinct()
                .ToList();

            if (invalidKeys.Count > 0)
                return BadRequest($"Invalid GroupKeys: {string.Join(", ", invalidKeys)}");

            try
            {
                var result = await _service.CreateLeaveTypeAsync(request);
                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("preview-entitlement-impact")]
        public async Task<IActionResult> PreviewEntitlementImpact(
    [FromBody] UpdateLeaveTypeRequest request)
        {
            if (request == null || request.Rules == null)
                return BadRequest("Invalid request.");

            try
            {
                var result = await _service
                    .PreviewEntitlementImpactAsync(request);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLeaveTypeRequest request)
        {
            var validGroupKeys = await _context.JobGradeGroupMaps
                .Select(x => x.GroupKey)
                .Distinct()
                .ToListAsync();

            var invalidKeys = request.Rules
                .Where(r => r.GroupKey != "ALL" && !validGroupKeys.Contains(r.GroupKey))
                .Select(r => r.GroupKey)
                .Distinct()
                .ToList();

            if (invalidKeys.Count > 0)
                return BadRequest($"Invalid GroupKeys: {string.Join(", ", invalidKeys)}");

            try
            {
                return Ok(await _service.UpdateLeaveTypeAsync(id, request));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployeesWithLeave()
            => Ok(await _service.GetAllEmployeesWithLeaveAsync());

        [HttpGet("employees/{employeeId}")]
        public async Task<IActionResult> GetEmployeeWithLeave(string employeeId)
        {
            var result = await _service.GetEmployeeWithLeaveByIdAsync(employeeId);

      if (result == null)
        return NotFound("Employee not found");

            return Ok(result);
        }
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var leaveType = await _context.LeaveTypes.FindAsync(id);

            if (leaveType == null)
                return NotFound();

            leaveType.IsActive = !leaveType.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new { leaveType.Id, leaveType.IsActive });
        }
    }
}
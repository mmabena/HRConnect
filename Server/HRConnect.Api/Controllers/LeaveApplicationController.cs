using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRConnect.Api.Data;
using HRConnect.Api.DTOs;
using HRConnect.Api.Models;

namespace HRConnect.Api.Controllers
{
    [ApiController]
    [Route("api/LeaveApplications")]
    public class LeaveApplicationsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public LeaveApplicationsController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> ApplyForLeave([FromForm] CreateApplicationRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.EmployeeId))
                return BadRequest("Invalid request");

            var application = new LeaveApplication
            {
                EmployeeId = request.EmployeeId,
                LeaveTypeId = request.LeaveTypeId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Description = request.Description ?? "",

                Status = LeaveApplication.LeaveApplicationStatus.Pending,
                AppliedDate = DateTime.UtcNow,

                ApprovalToken = Guid.NewGuid(),
                TokenExpiry = DateTime.UtcNow.AddHours(48)
            };

            _context.LeaveApplications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(application);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.LeaveApplications
                .Include(x => x.Employee)
                .Include(x => x.LeaveType)
                .Select(x => new LeaveApplicationResponse
                {
                    Id = x.Id,
                    EmployeeName = x.Employee.Name + " " + x.Employee.Surname,
                    LeaveTypeId = x.LeaveTypeId,
                    LeaveTypeCode = x.LeaveType.Code,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    DaysAllocated = _context.LeaveEntitlementRules
                        .Where(r => r.LeaveTypeId == x.LeaveTypeId && r.IsActive)
                        .OrderByDescending(r => r.MinYearsService)
                        .Select(r => r.DaysAllocated)
                        .FirstOrDefault(),
                    DaysRequested = x.DaysRequested,
                    Status = x.Status.ToString()
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("by-employee-id/{employeeId}")]
        public async Task<IActionResult> GetByEmployeeId(string employeeId)
        {
            var data = await _context.LeaveApplications
                .Where(x => x.EmployeeId == employeeId)
                .Include(x => x.LeaveType)
                .Select(x => new LeaveApplicationResponse
                {
                    Id = x.Id,
                    EmployeeName = x.Employee.Name + " " + x.Employee.Surname,
                    LeaveTypeId = x.LeaveTypeId,
                    LeaveTypeCode = x.LeaveType.Code,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    DaysAllocated = _context.LeaveEntitlementRules
                        .Where(r => r.LeaveTypeId == x.LeaveTypeId && r.IsActive)
                        .OrderByDescending(r => r.MinYearsService)
                        .Select(r => r.DaysAllocated)
                        .FirstOrDefault(),
                    DaysRequested = x.DaysRequested,
                    Status = x.Status.ToString()
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("{id}/approve")]
        public async Task<IActionResult> Approve(int id, [FromQuery] Guid token)
        {
            var application = await _context.LeaveApplications.FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
                return NotFound("Application not found");

            if (application.ApprovalToken != token)
                return BadRequest("Invalid token");

            if (application.TokenExpiry < DateTime.UtcNow)
                return BadRequest("Token expired");

            application.Status = LeaveApplication.LeaveApplicationStatus.Approved;

            await _context.SaveChangesAsync();

            return Ok("Leave application approved successfully");
        }

        [HttpGet("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromQuery] Guid token)
        {
            var application = await _context.LeaveApplications.FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
                return NotFound("Application not found");

            if (application.ApprovalToken != token)
                return BadRequest("Invalid token");

            if (application.TokenExpiry < DateTime.UtcNow)
                return BadRequest("Token expired");

            application.Status = LeaveApplication.LeaveApplicationStatus.Rejected;

            await _context.SaveChangesAsync();

            return Ok("Leave application rejected successfully");
        }

        [HttpPatch("{id}/approve-admin")]
        public async Task<IActionResult> ApproveByAdmin(int id)
        {
            var application = await _context.LeaveApplications.FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
                return NotFound("Application not found");

            if (application.Status != LeaveApplication.LeaveApplicationStatus.Pending)
                return BadRequest("Only pending applications can be approved");

            application.Status = LeaveApplication.LeaveApplicationStatus.Approved;
            application.DecisionDate = DateTime.UtcNow;
            application.DecisionBy = "Admin";

            await _context.SaveChangesAsync();

            return Ok(new { message = "Approved successfully" });
        }

        [HttpPatch("{id}/reject-admin")]
        public async Task<IActionResult> RejectByAdmin(int id, [FromBody] DecisionRequest request)
        {
            var application = await _context.LeaveApplications.FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
                return NotFound("Application not found");

            if (application.Status != LeaveApplication.LeaveApplicationStatus.Pending)
                return BadRequest("Only pending applications can be rejected");

            if (string.IsNullOrWhiteSpace(request?.Reason))
                return BadRequest("Rejection reason is required");

            application.Status = LeaveApplication.LeaveApplicationStatus.Rejected;
            application.RejectionReason = request.Reason;
            application.DecisionDate = DateTime.UtcNow;
            application.DecisionBy = "Admin";

            await _context.SaveChangesAsync();

            return Ok(new { message = "Rejected successfully" });
        }
    }
}
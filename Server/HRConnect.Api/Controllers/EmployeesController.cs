namespace HRConnect.Api.Controllers
{
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/employees")]
    [Authorize(Roles = "SuperUser")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IJobGradeRepository _jobGradeRepository;
        private readonly ILeaveEntitlementService _leaveEntitlementService;

        public EmployeesController(
            IEmployeeRepository employeeRepository,
            IJobGradeRepository jobGradeRepository,
            ILeaveEntitlementService leaveEntitlementService)
        {
            _employeeRepository = employeeRepository;
            _jobGradeRepository = jobGradeRepository;
            _leaveEntitlementService = leaveEntitlementService;
        }

        // ============================
        // CREATE EMPLOYEE
        // ============================
        [HttpPost]
        public async Task<IActionResult> CreateEmployee(CreateEmployeeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Surname) ||
                string.IsNullOrWhiteSpace(request.Gender) ||
                string.IsNullOrWhiteSpace(request.JobGradeName))
            {
                return BadRequest("Missing required fields.");
            }

            var employee = new Employee
            {
                Name = request.Name,
                Surname = request.Surname,
                Gender = request.Gender,
                ReportingManager = request.ReportingManager,
                JobGrade = request.JobGradeName,
                DateCreated = DateTime.UtcNow
            };

            await _employeeRepository.AddAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            var jobGrade = new JobGrade
            {
                EmployeeId = employee.EmployeeId,
                EmployeeName = $"{employee.Name} {employee.Surname}",
                ReportingManager = request.ReportingManager,
                JobGradeName = request.JobGradeName,
                CreatedDate = employee.DateCreated
            };

            await _jobGradeRepository.AddAsync(jobGrade);
            await _jobGradeRepository.SaveChangesAsync();

            // Allocate leave entitlements
            await _leaveEntitlementService.AllocateOnEmployeeHireAsync(employee.EmployeeId);

            return CreatedAtAction(
                nameof(GetEmployeeLeaveEntitlements),
                new { employeeId = employee.EmployeeId },
                employee);
        }

        // ============================
        // GET LEAVE ENTITLEMENTS
        // ============================
        [HttpGet("{employeeId}/leave-entitlements")]
        public async Task<IActionResult> GetEmployeeLeaveEntitlements(int employeeId)
        {
            var entitlements = await _employeeRepository.GetLeaveEntitlementsAsync(employeeId);

            if (entitlements == null || !entitlements.Any())
                return NotFound();

            return Ok(entitlements);
        }
    }
}

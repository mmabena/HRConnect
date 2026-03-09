
namespace HRConnect.Api.Controllers
{
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "SuperUser")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeEntitlementService _employeeService;

        public EmployeeController(IEmployeeEntitlementService employeeService)
        {
            _employeeService = employeeService;
        }
        [HttpPost]
        public async Task<ActionResult<EmployeeResponse>> Create(
            [FromBody] CreateEmployeeRequest request)
        {
            var result = await _employeeService.CreateEmployeeAsync(request);
            return CreatedAtAction(nameof(GetById),
                new { id = result.Id },
                result);
        }
        [HttpGet]
        public async Task<ActionResult<List<EmployeeResponse>>> GetAll()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EmployeeResponse>> GetById(Guid id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }
        [HttpPut("{id:guid}/position/{newPositionId:int}")]
        public async Task<ActionResult<EmployeeResponse>> UpdatePosition(
            Guid id,
            int newPositionId)
        {
            var updated = await _employeeService
                .UpdateEmployeePositionAsync(id, newPositionId);

            return Ok(updated);
        }
        
        [HttpPut("update-rule")]
        public async Task<IActionResult> UpdateRule(UpdateLeaveRuleRequest request)
        {
            await _employeeService.UpdateLeaveEntitlementRuleAsync(request);
            return Ok("Rule updated and employees recalculated.");
        }
        [HttpPut("update-used-days")]
        public async Task<IActionResult> UpdateUsedDays(
        [FromBody] UpdateUsedDaysRequest request)
        {
            await _employeeService.UpdateUsedDaysAsync(request);
            return Ok("Used days updated successfully.");
        }
        [HttpGet("project-annual-leave")]
        public async Task<IActionResult> ProjectAnnualLeave(
            Guid employeeId,
            DateOnly projectionDate)
        {
            var result = await _employeeService.ProjectAnnualLeaveAsync(
                employeeId,
                projectionDate);

            return Ok(result);
        }
    }
}

namespace HRConnect.Api.Controllers
{
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperUser")]
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
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeResponse>> GetById(string id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }
        [HttpPut("{id}/position/{newPositionId:int}")]
        public async Task<ActionResult<EmployeeResponse>> UpdatePosition(
            string id,
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
        public async Task<IActionResult> UpdateTakenDays(
        [FromBody] UpdateTakenDaysRequest request)
        {
            await _employeeService.UpdateTakenDaysAsync(request);
            return Ok("Used days updated successfully.");
        }
        /// <summary>
        /// Projects annual leave entitlement for a specific employee. This api will be used later to calculate termination leave payout based on projected entitlement at termination date. 
        /// The projection logic takes into account the employee's start date, any changes in their accrual rate throughout the year, 
        /// and the specified projection date to provide an accurate estimate of their leave entitlement at that future point in time.
        /// </summary>
        /// <param name="employeeId">The ID of the employee.</param>
        /// <param name="projectionDate">The date for which to project leave entitlement.</param>
        /// <returns>The projected leave entitlement.</returns>
        [HttpGet("project-annual-leave")]
        public async Task<IActionResult> ProjectAnnualLeave(
            string employeeId,
            DateOnly projectionDate)
        {
            var result = await _employeeService.ProjectAnnualLeaveAsync(
                employeeId,
                projectionDate);

            return Ok(result);
        }
    }
}

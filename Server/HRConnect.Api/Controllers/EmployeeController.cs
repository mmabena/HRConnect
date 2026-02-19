using HRConnect.Api.DTOs;
using HRConnect.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRConnect.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeEntitlementService _employeeService;

        public EmployeeController(IEmployeeEntitlementService employeeService)
        {
            _employeeService = employeeService;
        }

        // ============================
        // CREATE EMPLOYEE
        // ============================
        [HttpPost]
        public async Task<ActionResult<EmployeeResponse>> Create(
            [FromBody] CreateEmployeeRequest request)
        {
            var result = await _employeeService.CreateEmployeeAsync(request);
            return CreatedAtAction(nameof(GetById),
                new { id = result.Id },
                result);
        }

        // ============================
        // GET ALL
        // ============================
        [HttpGet]
        public async Task<ActionResult<List<EmployeeResponse>>> GetAll()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        // ============================
        // GET BY ID
        // ============================
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EmployeeResponse>> GetById(Guid id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        // ============================
        // UPDATE POSITION
        // ============================
        [HttpPut("{id:guid}/position/{newPositionId:int}")]
        public async Task<ActionResult<EmployeeResponse>> UpdatePosition(
            Guid id,
            int newPositionId)
        {
            var updated = await _employeeService
                .UpdateEmployeePositionAsync(id, newPositionId);

            return Ok(updated);
        }

        // ============================
        // DELETE
        // ============================
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return NoContent();
        }
        [HttpPost("process-carryover-notifications")]
        public async Task<IActionResult> TriggerCarryoverNotifications()
        {
            await _employeeService.ProcessCarryOverNotificationAsync();
            return Ok("Carryover notifications processed.");
        }

        [HttpPost("process-annual-reset")]
        public async Task<IActionResult> TriggerAnnualReset()
        {
            await _employeeService.ProcessAnnualResetAsync();
            return Ok("Annual reset processed.");
        }
        [HttpPut("update-rule")]
        public async Task<IActionResult> UpdateRule(UpdateLeaveRuleRequest request)
        {
            await _employeeService.UpdateLeaveEntitlementRuleAsync(request);
            return Ok("Rule updated and employees recalculated.");
        }

    }
}

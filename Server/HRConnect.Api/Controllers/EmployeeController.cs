
namespace HRConnect.Api.Controllers
{
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using Microsoft.AspNetCore.Mvc;

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
        //[HttpPost("process-carryover-notifications")]
        //public async Task<IActionResult> TriggerCarryoverNotifications()
        //{
        //    await _employeeService.ProcessCarryOverNotificationAsync();
        //            return Ok("Carryover notifications processed.");
        //}

        [HttpPost("process-annual-reset")]
        public async Task<IActionResult> TriggerAnnualReset(
     [FromQuery] int? overrideYear)
        {
            await _employeeService.ProcessAnnualResetAsync(overrideYear);
            return Ok($"Annual reset processed for year {overrideYear ?? DateTime.UtcNow.Year}.");
        }
        [HttpPut("update-rule")]
        public async Task<IActionResult> UpdateRule(UpdateLeaveRuleRequest request)
        {
            await _employeeService.UpdateLeaveEntitlementRuleAsync(request);
            return Ok("Rule updated and employees recalculated.");
        }
        //[HttpPost("{id:guid}/recalculate-sick")]
        ///public async Task<IActionResult> RecalculateSick(Guid id)
        //{
        //    await _employeeService.RecalculateSickLeaveAsync(id);
        //    return Ok("Sick leave recalculated.");
        //}
        [HttpPut("update-used-days")]
        public async Task<IActionResult> UpdateUsedDays(
        [FromBody] UpdateUsedDaysRequest request)
        {
            await _employeeService.UpdateUsedDaysAsync(request);
            return Ok("Used days updated successfully.");
        }
        //[HttpPost("{id:guid}/new-pregnancy")]
        //public async Task<IActionResult> RegisterNewPregnancy(Guid id)
        //{
        //   await _employeeService.ResetMaternityLeaveForNewPregnancy(id);
        //    return Ok("Maternity leave reset for new pregnancy.");
        //}
        //[HttpPost("{id:guid}/recalculate-frl")]
        //public async Task<IActionResult> RecalculateFRL(Guid id)
        //{
        //    await _employeeService.RecalculateFamilyResponsibilityLeaveAsync(id);
        //   return Ok("Family Responsibility Leave recalculated.");
        //}
        //[HttpPost("recalculate-all-frl")]
        //public async Task<IActionResult> RecalculateAllFRL()
        //{
        //    await _employeeService.RecalculateAllFamilyResponsibilityLeaveAsync();
        //    return Ok("All FRL recalculated.");
        //}
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

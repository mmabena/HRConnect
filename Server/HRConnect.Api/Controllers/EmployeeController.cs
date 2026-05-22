namespace HRConnect.Api.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.DTOs.Employee;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Mappers;
    using HRConnect.Api.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    [Route("api/employee")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILeaveBalanceService _leaveBalanceService;

        public EmployeeController(
            IEmployeeService employeeService,
            ILeaveBalanceService leaveBalanceService)
        {
            _employeeService = employeeService;
            _leaveBalanceService = leaveBalanceService;
        }

        [HttpGet]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        [HttpGet("{EmployeeId}")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> GetEmployeeById(string EmployeeId)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(EmployeeId);
            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        [HttpGet("email/{employeeEmail}")]
        [Authorize(Policy = "SuperOrNormalUser")]
        public async Task<IActionResult> GetEmployeeByEmail(string employeeEmail)
        {
            var employee = await _employeeService.GetEmployeeByEmailAsync(employeeEmail);
            return employee == null ? NotFound() : Ok(employee);
        }

        [HttpPost]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequestDto employeeDto)
        {
            var employee = await _employeeService.CreateEmployeeAsync(employeeDto);
            return CreatedAtAction(nameof(GetEmployeeById), new { employeeId = employee.EmployeeId }, employee);
        }

        [HttpPut("{EmployeeId}")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> UpdateEmployee(string EmployeeId, [FromBody] UpdateEmployeeRequestDto employeeDto)
        {
            var updatedEmployee = await _employeeService.UpdateEmployeeAsync(EmployeeId, employeeDto);
            if (updatedEmployee == null)
                return NotFound();

            return Ok(updatedEmployee);
        }

        // INJECTED: Update leave usage
        [HttpPut("update-used-days")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> UpdateTakenDays([FromBody] UpdateTakenDaysRequest request)
        {
            await _leaveBalanceService.UpdateTakenDaysAsync(request);
            return Ok("Used days updated successfully.");
        }

        // INJECTED: Leave projection
        [HttpGet("project-annual-leave")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> ProjectAnnualLeave(string employeeId, DateOnly projectionDate)
        {
            var result = await _leaveBalanceService.ProjectAnnualLeaveAsync(employeeId, projectionDate);
            return Ok(result);
        }

    
    /// <summary>
    /// Deletes a employee from the database (SuperUser only).
    /// </summary>
    /// <param name="EmployeeId">The employee ID</param>
    /// <returns>success message if employee is deleted successfully, NotFound if employee does not exist</returns>
    [HttpDelete("{EmployeeId}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> DeleteEmployee(string EmployeeId)
    {
      var deletedEmployee = await _employeeService.DeleteEmployeeAsync(EmployeeId);
      if (!deletedEmployee)
        return NotFound();

      return Ok("Employee deleted successfully.");
    }

  }
}
namespace HRConnect.Api.Controllers
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Models;
  using System.Globalization;
  using Microsoft.AspNetCore.Mvc;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Utils;
  using HRConnect.Api.DTOs.Employee;
  using Microsoft.AspNetCore.Authorization;
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Hubs;
  using Microsoft.AspNetCore.SignalR;

  [Route("api/employee")]
  [ApiController]
  public class EmployeeController : ControllerBase
  {
    private readonly IEmployeeService _employeeService;
    private readonly ILeaveBalanceService _leaveBalanceService;

    private readonly IHubContext<UserPositionHub> _userPositionHubContext;

    public EmployeeController(
        IEmployeeService employeeService,
        ILeaveBalanceService leaveBalanceService,
            IHubContext<UserPositionHub> userPositionHubContext)
    {
      _employeeService = employeeService;
      _leaveBalanceService = leaveBalanceService;
      _userPositionHubContext = userPositionHubContext;
    }

    [HttpGet]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetAllEmployees()
    {
      var userId = User.GetUserId();

      var employees = await _employeeService.GetAllEmployeesAsync(userId);
      return Ok(employees);
    }

    [HttpGet("{EmployeeId}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetEmployeeById(string EmployeeId)
    {
      var userId = User.GetUserId();

      var employee = await _employeeService.GetEmployeeByIdAsync(userId, EmployeeId);
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
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var userId = User.GetUserId();

      var employee = await _employeeService.CreateEmployeeAsync(userId, employeeDto);
      return CreatedAtAction(nameof(GetEmployeeById), new { employeeId = employee.EmployeeId }, employee);
    }

    [HttpPut("{EmployeeId}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> UpdateEmployee(string EmployeeId, [FromBody] UpdateEmployeeRequestDto employeeDto)
    {
      var userId = User.GetUserId();

      var updatedEmployee = await _employeeService.UpdateEmployeeAsync(userId, EmployeeId, employeeDto);
      if (updatedEmployee == null)
        return NotFound();

      await _userPositionHubContext.Clients.All.SendAsync(
          "ReceivePositionUpdate",

          EmployeeId, updatedEmployee.EmployeeId, updatedEmployee.PositionTitle);

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

      var userId = User.GetUserId();

      var deletedEmployee = await _employeeService.DeleteEmployeeAsync(userId, EmployeeId);
      if (!deletedEmployee)
        return NotFound();

      return Ok("Employee deleted successfully.");
    }

    [HttpPost("validate")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> ValidateEmployee([FromBody] CreateEmployeeRequestDto employeeDto)
    {

      var userId = User.GetUserId();
      await _employeeService.ValidateEmployeeAsync(
        userId,
        employeeDto);

      return Ok();

    }
  }
}
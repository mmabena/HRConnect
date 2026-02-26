namespace HRConnect.Api.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Mappers;
    using Microsoft.AspNetCore.Mvc;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.DTOs.Employee;
    using Microsoft.AspNetCore.Authorization;
    [Route("api/employee")]
    [ApiController]
    [Authorize(Policy = "SuperUserOnly")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }
        /// <summary>
        /// Retrieves all Employees from the database.
        /// </summary>
        /// <returns> A List of all Employees as EmployeeDTO objects</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }
        /// <summary>
        /// Retrieves a single employee by their Employee Id.
        /// </summary>
        /// <param name="EmployeeId">The employee identifier</param>
        /// <returns>The employee with the same Employee Id provided as EmployeeDTODto object or NotFound if the Employee Id does not exist</returns>
        [HttpGet("{EmployeeId}")]
        public async Task<IActionResult> GetEmployeeById(string EmployeeId)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(EmployeeId);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }
    /// <summary>
    /// Retrieves a single employee by their Employee email.
    /// </summary>
    /// <param name="employeeEmail">The employee email</param>
    /// <returns>The employee with the same Employee email provided as EmployeeDTODto object or NotFound if the Employee Id does not exist</returns>
    [Authorize(Policy = "NormalUserOnly")]
    [HttpGet("email/{employeeEmail}")]
    public async Task<IActionResult> GetEmployeeByEmail(string employeeEmail)
    {
      EmployeeDto? employee = await _employeeService.GetEmployeeByEmailAsync(employeeEmail);
      return employee == null ? NotFound() : Ok(employee);
    }
    /// <summary>
    /// Creates a new Employee in the database (SuperUsers only).
    /// </summary>
    /// <param name="employeeDto">The employee data request to create</param>
    /// <returns>The created employee as employeeDto or appropriate error response depending on e.g. missing fields or duplicate data</returns>
    [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequestDto employeeDto)
        {
            var employee = await _employeeService.CreateEmployeeAsync(employeeDto);
            return CreatedAtAction(nameof(GetEmployeeById), new { EmployeeId = employee.EmployeeId }, employee);
        }
        /// <summary>
        /// Updates an existing employee (SuperUser only).
        /// </summary>
        /// <param name="EmployeeId">The employee ID</param>
        /// <param name="employeeDto">The updated employee data</param>
        /// <returns>The updated employee as employeeDto, or error if not found, conflict and missing fields</returns>
        [HttpPut("{EmployeeId}")]
        public async Task<IActionResult> UpdateEmployee(string EmployeeId, [FromBody] UpdateEmployeeRequestDto employeeDto)
        {
            var updatedEmployee = await _employeeService.UpdateEmployeeAsync(EmployeeId, employeeDto);
            if (updatedEmployee == null)
                return NotFound();
            
            return Ok(updatedEmployee);
        }
        /// <summary>
        /// Deletes a employee from the database (SuperUser only).
        /// </summary>
        /// <param name="EmployeeId">The employee ID</param>
        /// <returns>success message if employee is deleted successfully, NotFound if employee does not exist</returns>
        [HttpDelete("{EmployeeId}")]
        public async Task<IActionResult> DeleteEmployee(string EmployeeId)
        {
            var deletedEmployee = await _employeeService.DeleteEmployeeAsync(EmployeeId);
            if (!deletedEmployee)
                return NotFound();

            return Ok("Employee deleted successfully.");
        }

    }
}
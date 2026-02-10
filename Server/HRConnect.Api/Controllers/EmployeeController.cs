namespace HRConnect.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Mappers;
    using Microsoft.AspNetCore.Mvc;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.DTOs.Employee;
    [Route("api/employee")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;

        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees.Select(e => e.ToEmployeeDto()));
        }

        [HttpGet("{EmployeeId}")]
        public async Task<IActionResult> GetEmployeeById(int EmployeeId)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(EmployeeId);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee.ToEmployeeDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequestDto employeeDto)
        {
            try
            {
                var created_emp = await _employeeService.CreateEmployeeAsync(employeeDto);
                return CreatedAtAction(nameof(GetEmployeeById), new { EmployeeId = created_emp.EmployeeId }, created_emp.ToEmployeeDto());
            }
            catch (ArgumentNullException ex)
            {
                ModelState.AddModelError("Validation", ex.Message);
                return ValidationProblem(ModelState);
            }
        }




    }
}
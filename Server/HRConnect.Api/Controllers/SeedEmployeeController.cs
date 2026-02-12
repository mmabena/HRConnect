namespace HRConnect.Api.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;

  [Route("api/seedemployee")]
  [ApiController]
  public class SeedEmployeeController : ControllerBase
  {
    private readonly IEmployeeService _seederService;
    private readonly IPayrollDeductionsService _payrollDeductionService;
    public SeedEmployeeController(IEmployeeService seederService, IPayrollDeductionsService payrollDeductionService)
    {
      _payrollDeductionService = payrollDeductionService;
      _seederService = seederService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEmployees()
    {
      List<Employee> employees = await _seederService.GetEmployeesAsync();
      return Ok(employees.Select(e => e.ToEmployeeDto()));
    }
    [HttpGet("{employeeId}")]
    public async Task<IActionResult> GetEmployeeById(int employeeId)
    {
      var employee = await _seederService.GetEmployeeByIdAsync(employeeId);
      if (employee == null) return NotFound();

      // employee = await _payrollDeductionService.AddDeductionsAsync(employee.EmployeeId);
      return Ok(employee.ToEmployeeDto());
    }

    [HttpGet("payrollDeductions")]
    public async Task<IActionResult> GetAllDeductions()
    {
      List<PayrollDeduction> deductions = await _payrollDeductionService.
      GetAllDeductionsAsync();
      return Ok(deductions.Select(d => d.ToPayrollDeductionsDto()));
    }
    [HttpGet("payrollDeductions/{employeeId}")]
    public async Task<IActionResult> GetDeductionsByEmployeeId(int employeeId)
    {
      var deductions = await _payrollDeductionService.GetDeductionsByEmployeeIdAsync(employeeId);
      if (deductions == null) return NotFound();

      return Ok(deductions.ToPayrollDeductionsDto());
    }
  }
}

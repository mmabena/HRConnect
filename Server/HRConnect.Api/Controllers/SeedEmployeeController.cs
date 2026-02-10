namespace HRConnect.Api.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Services;

  [Route("api/seedemployee")]
  [ApiController]
  public class SeedEmployeeController : ControllerBase
  {
    private readonly ISeederService _seederService;
    private readonly IPayrollDeductionService _payrollDeductionService;
    public SeedEmployeeController(ISeederService seederService, IPayrollDeductionService payrollDeductionService)
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

      employee = await _payrollDeductionService.DeductUifAsync(employee.EmployeeId);
      return Ok(employee.ToEmployeeDto());
    }
    [HttpGet("payrollDeductions")]
    public async Task<IActionResult> GetAllDeductions()
    {
      List<PayrollDeduction> deductions = await _payrollDeductionService.GetAllUifDeductions();
      return Ok(deductions.Select(d => d.ToPayrollDeductionsDto()));
    }
  }
}

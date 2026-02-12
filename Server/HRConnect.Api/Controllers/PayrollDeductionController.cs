namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Models;
  using Microsoft.AspNetCore.Mvc;

  [Route("api/payrollDeduction")]
  [ApiController]
  public class PayrollDeductionController : ControllerBase
  {
    private readonly IPayrollDeductionsService _payrollDeductionService;
    public PayrollDeductionController(IPayrollDeductionsService payrollDeductionService)
    {
      _payrollDeductionService = payrollDeductionService;
    }

    [HttpGet("{employeeId}")]
    public async Task<IActionResult> GetDeductionsByEmployeeId(int employeeId)
    {
      PayrollDeduction? deduction = await _payrollDeductionService.GetDeductionsByEmployeeIdAsync(employeeId);
      if (deduction == null) return NotFound();

      return Ok(deduction.ToPayrollDeductionsDto());
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDeductions()
    {
      List<PayrollDeduction> deductions = await _payrollDeductionService.GetAllDeductionsAsync();
      return Ok(deductions.Select(d => d.ToPayrollDeductionsDto()));
    }
    [HttpPost("{employeeId}")]
    public async Task<IActionResult> TakeDeductions(int employeeId)
    {
      try
      {
        var added_deduction = await _payrollDeductionService.AddDeductionsAsync(employeeId);
        return CreatedAtAction(nameof(GetDeductionsByEmployeeId),
        new { employeeId = added_deduction!.EmployeeId }, added_deduction);
      }
      catch (ArgumentException ex)
      {
        ModelState.AddModelError(string.Empty, ex.Message);
        return ValidationProblem(ModelState);
      }
    }
  }
}
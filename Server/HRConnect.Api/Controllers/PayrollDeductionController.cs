namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.DTOs.PayrollDeductions;
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
    public async Task<IActionResult> GetDeductionsByEmployeeId(string employeeId)
    {
      PayrollDeductionDto? deduction = await _payrollDeductionService.GetDeductionsByEmployeeIdAsync(employeeId);
      if (deduction == null) return NotFound();

      return Ok(deduction);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDeductions()
    {
      var deductions = await _payrollDeductionService.GetAllDeductionsAsync();
      return Ok(deductions);
    }
    [HttpPost("{employeeId}")]
    public async Task<IActionResult> TakeDeductions(string employeeId)
    {
      var added_deduction = await _payrollDeductionService.AddDeductionsAsync(employeeId);
      return CreatedAtAction(nameof(GetDeductionsByEmployeeId),
      new { employeeId = added_deduction!.EmployeeId }, added_deduction);
    }
  }
}
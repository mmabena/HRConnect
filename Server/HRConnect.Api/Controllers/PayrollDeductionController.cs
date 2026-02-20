<<<<<<< HEAD
namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.DTOs.PayrollDeduction;
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
      try
      {
        var added_deduction = await _payrollDeductionService.AddDeductionsAsync(employeeId);
        return CreatedAtAction(nameof(GetDeductionsByEmployeeId),
        new { employeeId = added_deduction!.EmployeeId }, added_deduction);
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }
  }
=======
namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.DTOs.PayrollDeduction;
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
      try
      {
        var added_deduction = await _payrollDeductionService.AddDeductionsAsync(employeeId);
        return CreatedAtAction(nameof(GetDeductionsByEmployeeId),
        new { employeeId = added_deduction!.EmployeeId }, added_deduction);
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }
  }
>>>>>>> ffbb278630612dc4ad31426eca5c3ec5c413ed4c
}
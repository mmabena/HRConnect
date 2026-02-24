namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.DTOs.StatutoryContribution;
  using Microsoft.AspNetCore.Mvc;

  [Route("api/statutoryContribution")]
  [ApiController]
  public class PayrollDeductionController : ControllerBase
  {
    private readonly IStatutoryContributionsService _statutoryContributionService;
    public PayrollDeductionController(IStatutoryContributionsService payrollDeductionService)
    {
      _statutoryContributionService = payrollDeductionService;
    }

    [HttpGet("{employeeId}")]
    public async Task<IActionResult> GetDeductionsByEmployeeId(string employeeId)
    {
      StatutoryContributionDto? deduction = await _statutoryContributionService.GetDeductionsByEmployeeIdAsync(employeeId);
      if (deduction == null) return NotFound();

      return Ok(deduction);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDeductions()
    {
      var deductions = await _statutoryContributionService.GetAllDeductionsAsync();
      return Ok(deductions);
    }
    [HttpPost("{employeeId}")]
    public async Task<IActionResult> TakeDeductions(string employeeId)
    {
      try
      {
        var added_deduction = await _statutoryContributionService.AddDeductionsAsync(employeeId);
        return CreatedAtAction(nameof(GetDeductionsByEmployeeId),
        new { employeeId = added_deduction!.EmployeeId }, added_deduction);
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }
  }
}
namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs.Payroll;
  using HRConnect.Api.Interfaces;
  using Microsoft.AspNetCore.Mvc;
  [Route("api/payroll")]
  [ApiController]
  public class PayrollController : ControllerBase
  {
    private readonly IPayrollPeriodService _payrollPeriodService;
    private readonly IPayrollRunService _payrollRunService;
    public PayrollController(IPayrollPeriodService payrollPeriodService, IPayrollRunService payrollRunService)
    {
      _payrollPeriodService = payrollPeriodService;
      _payrollRunService = payrollRunService;
    }

    [HttpGet("period")]
    public async Task<IActionResult> GetAllPeriods()
    {
      var periods = await _payrollPeriodService.GetAllPeriodsAsync();
      return Ok(periods);
    }

    [HttpGet("period/payrun/active")]
    public async Task<IActionResult> GetCurrentlyActiveRun()
    {
      var payrollRun = await _payrollRunService.GetCurrentRunAsync();
      if (payrollRun == null)
        return NotFound();
      return Ok(payrollRun);
    }


    [HttpGet("runs/byDate/")]
    public async Task<IActionResult> GetPayRunByDateRange([FromQuery] PayrollRunRequestDto dto)
    {
      var payrollRunDto = await _payrollRunService.RequestRunByDateAsync(dto);
      if (payrollRunDto == null)
        return NotFound();
      return Ok(payrollRunDto);

    }



    // [HttpGet("employee/{id}")]
    // public async Task<IActionResult> GetEmployeeMedicalAidDeductionById([FromRoute] string id)
    // {
    //   var deduction = await _medicalAidDeductionService.GetMedicalAidDeductionsByEmployeeIdAsync(id);
    //   return Ok(deduction);
    // }

    // [HttpPost("create/employee/{id}")]
    // public async Task<IActionResult> CreateNewEmployeeMedicalAidDeduction(
    //  [FromRoute] string id,
    //  [FromBody] CreateMedicalDeductionDto request)
    // {
    //   if (request == null)
    //   {
    //     return BadRequest("Request body is required with selected medical option details");
    //   }

    //   if (request.MedicalOptionId <= 0)
    //   {
    //     return BadRequest("Valid MedicalOptionId is required");
    //   }

    //   var deduction = await _medicalAidDeductionService.AddNewMedicalAidDeductions(
    //       id,
    //       request.MedicalOptionId,
    //       request);

    //   return CreatedAtAction(
    //       nameof(GetEmployeeMedicalAidDeductionById),
    //       new { id = deduction.EmployeeId },
    //       deduction);
    // }
  }
}
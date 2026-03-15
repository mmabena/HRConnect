namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.Interfaces;
  using Microsoft.AspNetCore.Mvc;
  using HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;
  [Route("api/payroll")]
  [ApiController]
  public class PayrollController : ControllerBase
  {
    private readonly IPayrollPeriodService _payrollPeriodService;
    private readonly IPayrollRunService _payrollRunService;
    private readonly IMedicalAidDeductionService _medicalAidDeductionService;
    public PayrollController(IPayrollPeriodService payrollPeriodService, IPayrollRunService payrollRunService, IMedicalAidDeductionService medicalAidDeductionService)
    {
      _payrollPeriodService = payrollPeriodService;
      _payrollRunService = payrollRunService;
      _medicalAidDeductionService = medicalAidDeductionService;
    }
    [HttpGet("period")]
    public async Task<IActionResult> GetAllPeriods()
    {
      var periods = await _payrollPeriodService.GetAllPeriodsAsync();
      return Ok(periods);
    }

    [HttpGet("period/payrun/{payrollRunNumber}")]
    public async Task<IActionResult> GetPayrollRunById(int payrollRunNumber)
    {
      var payrollRun = await _payrollRunService.GetPayrunByRunNumberAsync(payrollRunNumber);
      if (payrollRun == null)
        return NotFound();
      return Ok(payrollRun);
    }

    [HttpGet("period/payrun/active")]
    public async Task<IActionResult> GetCurrentlyActiveRun()
    {
      var payrollRun = await _payrollRunService.GetCurrentRunAsync();
      if (payrollRun == null)
        return NotFound();
      return Ok(payrollRun);
    }

    [HttpGet("records/{payrollRunNumber})")]
    public async Task<IActionResult> GetAllRecordsFromPayrunById(int payrollRunNumber)
    {
      var payrollRun = await _payrollRunService.GetAllPayRecordsFromPayRunAsync(payrollRunNumber);

      return Ok(payrollRun);
    }

    [HttpGet("employee/{id}")]
    public async Task<IActionResult> GetEmployeeMedicalAidDeductionById([FromRoute] string id)
    {
      var deduction = await _medicalAidDeductionService.GetMedicalAidDeductionsByEmployeeIdAsync(id);
      return Ok(deduction);
    }
    [HttpPost("create/employee/{id}")]
    public async Task<IActionResult> CreateNewEmployeeMedicalAidDeduction(
     [FromRoute] string id,
     [FromBody] CreateMedicalDeductionDto request)
    {
      if (request == null)
      {
        return BadRequest("Request body is required with selected medical option details");
      }

      if (request.MedicalOptionId <= 0)
      {
        return BadRequest("Valid MedicalOptionId is required");
      }

      var deduction = await _medicalAidDeductionService.AddNewMedicalAidDeductions(
          id,
          request.MedicalOptionId,
          request);
      Console.WriteLine($"~~~~~~~~~~~~~~~~~~~~~~~~~~~~PAYROLL RUN RECORD ADD");
      return CreatedAtAction(
          nameof(GetEmployeeMedicalAidDeductionById),
          new { id = deduction.EmployeeId },
          deduction);
    }
  }
}
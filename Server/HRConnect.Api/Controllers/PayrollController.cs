namespace HRConnect.Api.Controllers
{
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

    // [HttpPost("{employeeId}")]
    // public async Task<IActionResult> AddRecordToCurrentRun(string employeeId)
    // {
    //   var currentRun = await _payrollRunService.GetCurrentRunAsync();
    //   if (currentRun == null)
    //     return NotFound("No active payroll run found.");
    //   // Create a new payroll record for the employee
    //   var payrollRecord = new Models.PayrollDeduction.MedicalAidDeduction
    //   {
    //     EmployeeId = employeeId,
    //     ChildPremium = 200m
    //   };
    //   await _payrollRunService.AddRecordToCurrentRunAsync(payrollRecord, employeeId);
    //   return Ok("Payroll record added to current run.");
    // }

    [HttpGet("records/{payrollRunNumber})")]
    public async Task<IActionResult> GetAllRecordsFromPayrunById(int payrollRunNumber)
    {
      var payrollRun = await _payrollRunService.GetAllPayRecordsFromPayRunAsync(payrollRunNumber);

      return Ok(payrollRun);
    }

    // [HttpPost("testentity/{name}")]
    // public async Task<IActionResult> AddTestEntityToCurrentRun(string name)
    // {
    //   var currentRun = await _payrollRunService.GetCurrentRunAsync();
    //   if (currentRun == null)
    //     return NotFound("No active payroll run found.");

    //   Console.WriteLine($">>><<<><>>><><><CURRENT RUN PAYRUN ID=={currentRun.PayrollRunNumber}");
    //   var testEntity = new Models.PayrollDeduction.TestEntity
    //   {
    //     Name = name,
    //     PayrollRunId = currentRun.PayrollRunId
    //   };

    //   try
    //   {
    //     await _context.TestEntities.AddAsync(testEntity);
    //     await _context.SaveChangesAsync();
    //   }
    //   catch (Exception ex)
    //   {
    //     Console.WriteLine($">>>>>>>>FAILED TO USE FK FOR TEST\n {ex}");
    //   }
    //   return Ok("Test entity added to current run.");
    // }

    // [HttpPost("employee/{id}/eligible-options")]
    // public async Task<IActionResult> GetEligibleMedicalOptions(
    //      [FromRoute] string id,
    //      [FromBody] RequestEligibileOptionsDto request)
    // {
    //   if (request == null)
    //   {
    //     return BadRequest("Request body is required");
    //   }

    //   var eligibleOptions = await _eligibilityService.GetEligibleMedicalOptionsForEmployeeAsync(id, request);
    //   return Ok(eligibleOptions);
    // }

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
    //   Console.WriteLine($"~~~~~~~~~~~~~~~~~~~~~~~~~~~~PAYROLL RUN RECORD ADD");
    //   return CreatedAtAction(
    //       nameof(GetEmployeeMedicalAidDeductionById),
    //       new { id = deduction.EmployeeId },
    //       deduction);
    // }
  }
}
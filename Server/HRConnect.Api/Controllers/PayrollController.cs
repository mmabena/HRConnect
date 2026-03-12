namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.Interfaces;
  using Microsoft.AspNetCore.Mvc;
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.DTOs;

  [Route("api/payroll")]
  [ApiController]
  public class PayrollController : ControllerBase
  {
    private readonly IPayrollPeriodService _payrollPeriodService;
    private readonly IPayrollRunService _payrollRunService;
    private readonly IMedicalAidDeductionService _medicalAidDeductionService;
    private readonly IMedicalAidEligibilityService _eligibilityService;
    private readonly ApplicationDBContext _context;
    public PayrollController(IPayrollPeriodService payrollPeriodService, IPayrollRunService payrollRunService, IMedicalAidDeductionService medicalAidDeductionService, IMedicalAidEligibilityService eligibilityService
    , ApplicationDBContext context)
    {
      _payrollPeriodService = payrollPeriodService;
      _payrollRunService = payrollRunService;
      _medicalAidDeductionService = medicalAidDeductionService;
      _eligibilityService = eligibilityService;
      _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> GetAllPeriods()
    {
      var periods = await _payrollPeriodService.GetAllPeriodsAsync();
      return Ok(periods);
    }

    [HttpGet("{payrollRunNumber}")]
    public async Task<IActionResult> GetPayrollRunById(int payrollRunNumber)
    {
      var payrollRun = await _payrollRunService.GetPayrunByIdAsync(payrollRunNumber);
      if (payrollRun == null)
        return NotFound();
      return Ok(payrollRun);
    }
    [HttpPost("{employeeId}")]
    public async Task<IActionResult> AddRecordToCurrentRun(string employeeId)
    {
      var currentRun = await _payrollRunService.GetCurrentRunAsync();
      if (currentRun == null)
        return NotFound("No active payroll run found.");
      // Create a new payroll record for the employee
      var payrollRecord = new Models.PayrollDeduction.MedicalAidDeduction
      {
        EmployeeId = employeeId,
        ChildPremium = 200m
      };
      await _payrollRunService.AddRecordToCurrentRunAsync(payrollRecord, employeeId);
      return Ok("Payroll record added to current run.");
    }
    [HttpGet("records/{payrollRunId})")]
    public async Task<IActionResult> GetAllRecordsFromPayrunById(int payrollRunId)
    {
      var payrollRun = await _payrollRunService.GetAllPayRecordsFromPayRunAsync(payrollRunId);

      return Ok(payrollRun);
    }

    //Testing an entity that isn't a record can use PayrollRunId as a FK
    [HttpPost("testentity/{name}")]
    public async Task<IActionResult> AddTestEntityToCurrentRun(string name)
    {
      var currentRun = await _payrollRunService.GetCurrentRunAsync();
      if (currentRun == null)
        return NotFound("No active payroll run found.");

      Console.WriteLine($">>><<<><>>><><><CURRENT RUN PAYRUN ID=={currentRun.PayrollRunNumber}");
      var testEntity = new Models.PayrollDeduction.TestEntity
      {
        Name = name,
        PayrollRunId = currentRun.PayrollRunId
      };

      // Assuming you have a repository method to add a TestEntity
      // await _testEntityRepository.AddTestEntityAsync(testEntity);
      try
      {
        await _context.TestEntities.AddAsync(testEntity);
        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        Console.WriteLine($">>>>>>>>FAILED TO USE FK FOR TEST\n {ex}");
      }
      return Ok("Test entity added to current run.");
    }

    [HttpPost("employee/{id}/eligible-options")]
    public async Task<IActionResult> GetEligibleMedicalOptions(
         [FromRoute] string id,
         [FromBody] RequestEligibileOptionsDto request)
    {
      if (request == null)
      {
        return BadRequest("Request body is required");
      }

      var eligibleOptions = await _eligibilityService.GetEligibleMedicalOptionsForEmployeeAsync(id, request);
      return Ok(eligibleOptions);
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
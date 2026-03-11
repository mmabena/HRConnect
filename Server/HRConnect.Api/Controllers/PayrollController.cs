namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.Interfaces;
  using Microsoft.AspNetCore.Mvc;
  using HRConnect.Api.Data;
  [Route("api/payroll")]
  [ApiController]
  public class PayrollController : ControllerBase
  {
    private readonly IPayrollPeriodService _payrollPeriodService;
    private readonly IPayrollRunService _payrollRunService;
    private readonly ApplicationDBContext _context;
    public PayrollController(IPayrollPeriodService payrollPeriodService, IPayrollRunService payrollRunService
    , ApplicationDBContext context)
    {
      _payrollPeriodService = payrollPeriodService;
      _payrollRunService = payrollRunService;
      _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> GetAllPeriods()
    {
      var periods = await _payrollPeriodService.GetAllPeriodsAsync();
      return Ok(periods);
    }

    [HttpGet("{payrollRunId}")]
    public async Task<IActionResult> GetPayrollRunById(int payrollRunId)
    {
      var payrollRun = await _payrollRunService.GetPayrunByIdAsync(payrollRunId);
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
      await _payrollRunService.AddRecordToCurrentRunAsync(payrollRecord);
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

      Console.WriteLine($">>><<<><>>><><><CURRENT RUN PAYRUN ID=={currentRun.PayrollRunId}");
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
  }
}
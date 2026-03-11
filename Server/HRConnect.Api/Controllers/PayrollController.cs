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
  }
}
namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.Interfaces;
  using Microsoft.AspNetCore.Mvc;

  [Route("api/payroll")]
  [ApiController]
  public class PayrollController : ControllerBase
  {
    private readonly IPayrollPeriodService _payrollPeriodService;
    public PayrollController(IPayrollPeriodService payrollPeriodService)
    {
      _payrollPeriodService = payrollPeriodService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAllPeriods()
    {
      var periods = await _payrollPeriodService.GetAllPeriodsAsync();
      return Ok(periods);
    }
  }
}
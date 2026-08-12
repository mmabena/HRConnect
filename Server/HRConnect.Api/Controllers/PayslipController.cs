namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.Interfaces.Payroll;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class PayslipController(
      IPayslipService payslipService) : ControllerBase
  {

    [HttpGet("history/{employeeId}")]
    public async Task<IActionResult> GetPayslipHistory(
        string employeeId,
        CancellationToken cancellationToken)
    {
      var history =
          await payslipService.GetPayslipHistoryAsync(
              employeeId,
              cancellationToken);

      if (history == null || !history.Any())
      {
        return NotFound(new
        {
          message =
              "No payslip history found for this employee."
        });
      }

      return Ok(history);
    }

    [HttpGet(
        "{employeeId}/{payrollRunId}/{payrollRunNumber}")]
    public async Task<IActionResult> GetPayslip(
        string employeeId,
        int payrollRunId,
        int payrollRunNumber,
        CancellationToken cancellationToken)
    {
      var payslip =
          await payslipService.GetPayslipAsync(
              employeeId,
              payrollRunId,
              payrollRunNumber,
              cancellationToken);

      if (payslip == null)
      {
        return NotFound(new
        {
          message = "Payslip not found."
        });
      }

      return Ok(payslip);
    }
  }
}
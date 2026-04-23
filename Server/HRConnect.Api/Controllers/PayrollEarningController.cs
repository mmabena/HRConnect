namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs.Payroll.Earning;
  using HRConnect.Api.Interfaces.Payroll.Earning;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  [Route("api/payrollearning")]
  [ApiController]
  [Authorize(Roles = "SuperUser")]
  public class PayrollEarningController(IPayrollEarningService payrollEarningService) : ControllerBase
  {
    private readonly IPayrollEarningService _payrollEarningService = payrollEarningService;

    ///<summary>
    ///Add Payroll Earning
    ///</summary>
    ///<param name="payrollEarningAddDto">Payroll Earning Request Data Transfer Object</param>
    ///<returns>
    ///IActionResult with added payroll earning details
    ///</returns>
    [HttpPost]
    public async Task<IActionResult> AddPayrollEarning(PayrollEarningAddDto payrollEarningAddDto)
    {
      PayrollEarningDto payrollEarningDto = await _payrollEarningService.AddPayrollEarningAsync(payrollEarningAddDto);
      return Ok(payrollEarningDto);
    }

    ///<summary>
    ///Get All Payroll Earnings
    ///</summary>
    ///<returns>
    ///IActionResult with a list of all payroll earnings
    ///</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllPayrollEarnings()
    {
      List<PayrollEarningDto> payrollEarnings = await _payrollEarningService.GetAllPayrollEarningsAsync();
      return Ok(payrollEarnings);
    }

    ///<summary>
    ///Get payroll earning details by payroll earning Id
    ///</summary>
    ///<param name="payrollEarningId">Pay Roll Earning Id</param>
    ///<returns>
    ///IActionResult with payroll earning details of the given payroll earning Id
    ///</returns>
    [HttpGet]
    [Route("id/{payrollEarningId}")]
    public async Task<IActionResult> GetPayrollEarningById([FromRoute] string payrollEarningId)
    {
      PayrollEarningDto? payrollEarning = await _payrollEarningService.GetPayrollEarningByIdAsync(payrollEarningId);
      return Ok(payrollEarning);
    }

    ///<summary>
    ///Get payroll earning details by tax code
    ///</summary>
    ///<param name="taxCode">Tax Code</param>
    ///<returns>
    ///IActionResult with payroll earning details of the given tax code
    ///</returns>
    [HttpGet]
    [Route("code/{taxCode}")]
    public async Task<IActionResult> GetPayrollEarningByTaxCode([FromRoute] int taxCode)
    {
      List<PayrollEarningDto> payrollEarnings = await _payrollEarningService.GetPayrollEarningByTaxCode(taxCode);
      return Ok(payrollEarnings);
    }

    ///<summary>
    ///Update payroll earning details
    ///</summary>
    ///<param name="payrollEarningUpdateDto">Payroll Earning Update Request Data Transfer Object</param>
    ///<returns>
    ///IActionResult with updated payroll earning details
    ///</returns>
    [HttpPut]
    public async Task<IActionResult> UpdatePayrollEarning(PayrollEarningUpdateDto payrollEarningUpdateDto)
    {
      PayrollEarningDto payrollEarningDto = await _payrollEarningService.UpdatePayrollEarningAsync(payrollEarningUpdateDto);
      return Ok(payrollEarningDto);
    }

    ///<summary>
    ///Set payroll earning to inactive
    ///</summary>
    ///<param name="payrollEarningId">Payroll Earning Id</param>
    ///<returns>
    ///IActionResult with a message indicating the payroll earning has been set to inactive
    ///</returns>
    [HttpPatch]
    [Route("inactivate/{payrollEarningId}")]
    public async Task<IActionResult> SetPayrollEarningToInactive([FromRoute] string payrollEarningId)
    {
      string response = await _payrollEarningService.SetPayrollEarningToInactiveAsync(payrollEarningId);
      return Ok(response);
    }
  }
}

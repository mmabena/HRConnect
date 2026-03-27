namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs.Payroll.Pension;
  using HRConnect.Api.Interfaces.Pension;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  [Route("api/pensiondeduction")]
  [ApiController]
  [Authorize(Roles = "SuperUser")]
  public class PensionDeductionController(IPensionDeductionService pensionDeductionService) : ControllerBase
  {
    private readonly IPensionDeductionService _pensionDeductionService = pensionDeductionService;

    ///<summary>
    ///Add Employees Pension Deduction
    ///</summary>
    ///<param name="pensionDeductionAddDto">Employee's Pension Deduction Request Data Transfer Object</param>
    ///<returns>
    ///IActionResult with employees added pension deduction details
    ///</returns>
    [HttpPost("pensiondeduct")]
    public async Task<IActionResult> AddEmployeesPensionDeduction(PensionDeductionAddDto pensionDeductionAddDto)
    {
      PensionDeductionDto? pensionDeductionDto = await _pensionDeductionService.AddPensionDeductionAsync(pensionDeductionAddDto);
      return Ok(pensionDeductionDto);
    }

    ///<summary>
    ///Get All Pension Deductions
    ///</summary>
    ///<returns>
    ///IActionResult with list of all pension deductions details
    ///</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllPensionDeductions()
    {
      List<PensionDeductionDto> pensionDeductions = await _pensionDeductionService.GetAllPensionDeductionsAsync();
      return Ok(pensionDeductions);
    }

    ///<summary>
    ///Get Employee's latest pension deduction by employee Id
    ///</summary>
    ///<param name="employeeId">Employee's Id</param>
    ///<returns>
    ///IActionResult with employee's latest pension deduction details
    ///</returns>
    [HttpGet]
    [Route("employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeePensionDeductionsById([FromRoute] string employeeId)
    {
      PensionDeductionDto? employeePensionDeduction = await _pensionDeductionService.GetEmployeePensionDeductionByIdAsync(employeeId);

      return Ok(employeePensionDeduction);
    }

    ///<summary>
    ///Get all pension deductions by payroll run Id
    ///</summary>
    ///<param name="payrollRunId">Pay Roll Run Id</param>
    ///<returns>
    ///IActionResult with list of pension deductions details for the specified payroll run Id
    ///</returns>
    [HttpGet]
    [Route("payrun/{payrollRunId}")]
    public async Task<IActionResult> GetPensionDeductionsByPayRollRunId([FromRoute] int payrollRunId)
    {
      List<PensionDeductionDto> pensionDeductions = await _pensionDeductionService
        .GetPensionDeductionsByPayRollRunIdAsync(payrollRunId);
      return Ok(pensionDeductions);
    }

    ///<summary>
    ///Update Employee's pension deduction details
    ///</summary>
    ///<param name="pensionDeductionUpdateDto">Pension Deduction Update Request Data Transfer Object</param>
    ///<returns>
    ///IActionResult with employee's updated pension deduction details
    ///</returns>
    [HttpPut]
    public async Task<IActionResult> UpdateEmployeePensionEnrollment(PensionDeductionUpdateDto pensionDeductionUpdateDto)
    {
      PensionDeductionDto? updatePensionDeduction = await _pensionDeductionService.
        UpdateEmployeePensionDeductionAsync(pensionDeductionUpdateDto);

      return Ok(updatePensionDeduction);
    }
  }
}

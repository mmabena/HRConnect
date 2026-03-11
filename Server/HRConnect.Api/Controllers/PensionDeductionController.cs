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

    [HttpPost("pensiondeduct")]
    public async Task<IActionResult> AddEmployeesPensionDeduction(PensionDeductionAddDto pensionDeductionAddDto)
    {
      PensionDeductionDto? pensionDeductionDto = await _pensionDeductionService.AddPensionDeductionAsync(pensionDeductionAddDto);
      return Ok(pensionDeductionDto);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPensionDeductions()
    {
      List<PensionDeductionDto> pensionDeductions = await _pensionDeductionService.GetAllPensionDeductionsAsync();
      return Ok(pensionDeductions);
    }

    [HttpGet]
    [Route("employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeePensionDeductionsById([FromRoute] string employeeId)
    {
      PensionDeductionDto? employeePensionDeduction = await _pensionDeductionService.GetEmployeePensionDeductionByIdAsync(employeeId);

      return Ok(employeePensionDeduction);
    }

    [HttpGet]
    [Route("payrun/{payrollRunId}")]
    public async Task<IActionResult> GetPensionDeductionsByPayRollRunId([FromRoute] int payrollRunId)
    {
      List<PensionDeductionDto> pensionDeductions = await _pensionDeductionService
        .GetPensionDeductionsByPayRollRunIdAsync(payrollRunId);
      return Ok(pensionDeductions);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateEmployeePensionEnrollment(PensionDeductionUpdateDto pensionDeductionUpdateDto)
    {
      PensionDeductionDto? updatePensionDeduction = await _pensionDeductionService.
        UpdateEmployeePensionDeductionAsync(pensionDeductionUpdateDto);

      return Ok(updatePensionDeduction);
    }
  }
}

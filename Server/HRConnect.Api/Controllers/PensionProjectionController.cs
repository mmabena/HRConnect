namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.Interfaces.Finance;
  using HRConnect.Api.Utils;
  using Microsoft.AspNetCore.Mvc;

  [Route("api/pension")]
  [ApiController]
  public class PensionProjectionController(IPensionProjectionService pensionProjectionService) : ControllerBase
  {
    private readonly IPensionProjectionService _pensionProjectionService = pensionProjectionService;

    [HttpGet("options")]
    public async Task<IActionResult> GetPensionOptions()
    {
      float[] pensionOptions = await Task.FromResult(PensionOption.options);

      return Ok(pensionOptions);
    }

    [HttpPost("projection")]
    public async Task<IActionResult> ProjectPensionFund([FromBody] PensionProjectionRequestDto pensionProjectionRequestDto)
    {
      PensionProjectionResultDto pensionProjectionResultDto = await Task.FromResult(_pensionProjectionService.ProjectPension(pensionProjectionRequestDto));

      return Ok(pensionProjectionResultDto);
    }

    [HttpGet]
    public IActionResult Connected()
    {
      return Ok("You are connected");
    }

  }
}
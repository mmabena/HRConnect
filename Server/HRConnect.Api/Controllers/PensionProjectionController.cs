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
    ///<summary>
    ///Project options
    ///</summary>
    ///<returns>
    ///All pension percentage options
    ///</returns>
    private readonly IPensionProjectionService _pensionProjectionService = pensionProjectionService;
    [HttpGet("options")]
    public async Task<IActionResult> GetPensionOptions()
    {
      float[] pensionOptions = await Task.FromResult(PensionOption.options);

      return Ok(pensionOptions);
    }

    ///<summary>
    ///Project pension
    ///</summary>
    ///<param name="pensionProjectRequestDto">Pension Project Request Data Transfer Object</param>
    ///<returns>
    ///IActionResult with projected pension data
    ///</returns>
    [HttpPost("projection")]
    public async Task<IActionResult> ProjectPensionFund([FromBody] PensionProjectionRequestDto pensionProjectionRequestDto)
    {
      PensionProjectionResultDto pensionProjectionResultDto = await Task.FromResult(_pensionProjectionService.ProjectPension(pensionProjectionRequestDto));

      return Ok(pensionProjectionResultDto);
    }
  }
}
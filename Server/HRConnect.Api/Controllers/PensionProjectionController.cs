namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.Interfaces.PensionProjection;
  using Microsoft.AspNetCore.Mvc;

  [Route("api/pension")]
  [ApiController]
  public class PensionProjectionController(IPensionProjectionService pensionProjectionService) : ControllerBase
  {
    private readonly IPensionProjectionService _pensionProjectionService = pensionProjectionService;
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
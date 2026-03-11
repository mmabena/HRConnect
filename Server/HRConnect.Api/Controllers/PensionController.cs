namespace HRConnect.Api.Controllers
{
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  [Route("api/[controller]")]
  public class PensionController(IPensionFundService pensionFundService) : ControllerBase
  {

    [HttpGet("options")]
    public async Task<ActionResult<IEnumerable<PensionOption>>> GetPensionOptions(CancellationToken cancellationToken)
    {
      IEnumerable<PensionOption> options = await pensionFundService.GetPensionOptionsAsync(cancellationToken);
      return Ok(options);
    }

    [HttpGet("options/{id}")]
    public async Task<ActionResult<PensionOption>> GetPensionOption(int id, CancellationToken cancellationToken)
    {
      PensionOption? option = await pensionFundService.GetPensionOptionByIdAsync(id, cancellationToken);
      return option != null ? Ok(option) : NotFound("Pension option not found.");
    }

    [Authorize(Roles = "SuperUser")]
    [HttpPost("options")]
    public async Task<IActionResult> AddPensionOption([FromBody] PensionOptionDto dto, CancellationToken cancellationToken)
    {
      PensionOption pensionOption = new()
      {
        ContributionPercentage = dto.ContributionPercentage
      };

      ServiceResult result = await pensionFundService.AddPensionOptionAsync(pensionOption, cancellationToken);

      return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }

    [Authorize(Roles = "SuperUser")]
    [HttpPut("options")]
    public async Task<IActionResult> UpdatePensionOption([FromBody] PensionOptionDto dto, CancellationToken cancellationToken)
    {
      PensionOption pensionOption = new()
      {
        PensionOptionId = dto.PensionOptionId,
        ContributionPercentage = dto.ContributionPercentage
      };

      ServiceResult result = await pensionFundService.UpdatePensionOptionAsync(pensionOption, cancellationToken);

      return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }

    [HttpPost("select-option")]
    public async Task<IActionResult> SelectPensionOption([FromBody] SelectPensionOptionDto dto, CancellationToken cancellationToken)
    {
      ServiceResult result = await pensionFundService.RecordEmployeePensionSelectionAsync(
          dto.EmployeeId,
          dto.PensionOptionId,
          cancellationToken);

      return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }
  }
}
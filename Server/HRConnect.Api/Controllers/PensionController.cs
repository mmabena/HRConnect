namespace HRConnect.Api.Controllers
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Services;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  [Route("api/[controller]")]
  public class PensionController(IPensionFundService pensionFundService) : ControllerBase
  {
    private readonly IPensionFundService pensionFundService = pensionFundService;

    // ===============================
    // GET - All Pension Options
    // ===============================
    [HttpGet("options")]
    public async Task<ActionResult<IEnumerable<PensionOption>>> GetPensionOptions()
    {
      IEnumerable<PensionOption> options = await pensionFundService.GetPensionOptionsAsync();
      return Ok(options);
    }

    // ===============================
    // GET - Single Pension Option
    // ===============================
    [HttpGet("options/{id}")]
    public async Task<ActionResult<PensionOption>> GetPensionOption(int id)
    {
      PensionOption? option = await pensionFundService.GetPensionOptionByIdAsync(id);
      return option != null ? Ok(option) : NotFound("Pension option not found.");
    }

    // ===============================
    // POST - Add Pension Option
    // ===============================
    [Authorize(Roles = "SuperUser")]
    [HttpPost("options")]
    public async Task<IActionResult> AddPensionOption([FromBody] PensionOptionDto dto)
    {
      PensionOption pensionOption = new()
      {
        ContributionPercentage = dto.ContributionPercentage
      };

      ServiceResult result = await pensionFundService.AddPensionOptionAsync(pensionOption);
      return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }

    // ===============================
    // PUT - Update Pension Option
    // ===============================
    [HttpPut("options")]
    public async Task<IActionResult> UpdatePensionOption([FromBody] PensionOptionDto dto)
    {
      PensionOption pensionOption = new()
      {
        PensionOptionId = dto.PensionOptionId,
        ContributionPercentage = dto.ContributionPercentage
      };

      ServiceResult result = await pensionFundService.UpdatePensionOptionAsync(pensionOption);
      return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }

    // ===============================
    // POST - Employee Selects Pension Option
    // ===============================
    [HttpPost("select-option")]
    public async Task<IActionResult> SelectPensionOption([FromBody] SelectPensionOptionDto dto)
    {
      ServiceResult result = await pensionFundService.RecordEmployeePensionSelectionAsync(dto.EmployeeId, dto.PensionOptionId);
      return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
    }
  }

}


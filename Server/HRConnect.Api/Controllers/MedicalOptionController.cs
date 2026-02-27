namespace HRConnect.Api.Controllers
{

  using System.Linq;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.DTOs.MedicalOption;
  using Microsoft.AspNetCore.Mvc;
  
  [Route("api/medical-options")]
  [ApiController]
  public class MedicalOptionController:Controller
  {
    private readonly IMedicalOptionService _medicalOptionService;
    public MedicalOptionController(IMedicalOptionService medicalOptionService)
    {
      _medicalOptionService = medicalOptionService;
    }
    
    [HttpGet("categories")]
    //[Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetGroupedMedicalOptions()
    {
      //With middleware global exception handling, we can remove the try catch block and
      //just have the execution logic here.
      //The middleware will catch any unhandled exceptions and return a standardized error response.

      var groupedOptions = await _medicalOptionService
        .GetGroupedMedicalOptionsAsync();

      if (groupedOptions == null)
      {
        return NotFound();
      }

      return Ok(groupedOptions);
    }

    [HttpPut("{optionId}/salary-bracket")]
    //[Authorize(Roles = "SuperUser")]
    public async Task<ActionResult<IReadOnlyList<MedicalOptionDto>>> BulkUpdateMedicalOptionsByCategory(
      int optionId, 
      [FromBody] IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto)
    {
      var result =
        await _medicalOptionService.BulkUpdateMedicalOptionsByCategoryAsync(optionId,
          bulkUpdateDto);

      if (result == null || !result.Any()) return NotFound();

      return NoContent();
    }
  }
}
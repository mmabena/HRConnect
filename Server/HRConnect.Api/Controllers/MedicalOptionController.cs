namespace HRConnect.Api.Controllers
{

  using System.Linq;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.DTOs.MedicalOption;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Http.HttpResults;
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
      //TODO: Wrap in a try catch block for error handling
      try
      {
        var groupedOptions = await _medicalOptionService
          .GetGroupedMedicalOptionsAsync();
        
        if(groupedOptions == null)
        {
          return NotFound();
        }
        
        return Ok(groupedOptions);
      }
      catch(Exception ex)
      {
        return StatusCode(503, ex.Message); 
      }
    }

    [HttpPut("{optionId}/salary-bracket")]
    //[Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> UpdateMedicalOptionSalaryBracket([FromRoute] int optionId,
      [FromBody] HRConnect.Api.DTOs.MedicalOption.UpdateMedicalOptionSalaryBracketRequestDto
        requestDto)
    {
      try
      {
        var results = await _medicalOptionService.UpdateSalaryBracketAsync(optionId,
          requestDto);

        if (results == null) return NotFound();

        return Created();
      } // TODO : Cater for other Exception Types that are thrown in the service layer
      catch (ArgumentException ex)
      {
        ModelState.AddModelError("Validation", ex.Message);
        return ValidationProblem(ModelState);
      }
      catch (InvalidOperationException ex)
      {
        //ModelState.AddModelError("Unauthorized Operation", ex.Message);
        return NotFound($"Unauthorized operation: {ex.Message}");
      }
    }

  }
}
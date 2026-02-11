namespace HRConnect.Api.Controllers
{

  using System.Linq;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Interfaces;
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
      var groupedOptions = await _medicalOptionService.GetGroupedMedicalOptionsAsync();
      
      return Ok(groupedOptions);
    }
  }
}
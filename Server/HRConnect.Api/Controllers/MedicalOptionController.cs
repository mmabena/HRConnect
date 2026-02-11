namespace HRConnect.Api.Controllers
{

  using HRConnect.Api.Mappers;
  using HRConnect.Api.Interfaces;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Http.HttpResults;
  using Microsoft.AspNetCore.Mvc;
  
  [Route("api/medical-options")]
  [ApiController]
  public class MedicalOptionController
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
      return null;
    }
  }
}
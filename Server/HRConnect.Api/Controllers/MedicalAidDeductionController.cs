namespace HRConnect.Api.Controllers
{
  using System.Linq;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.DTOs.MedicalOption;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  
  [Route("api/medical-aid-deductions")]
  [ApiController]
  public class MedicalAidDeductionController:ControllerBase
  {
    private readonly IMedicalAidDeductionService _medicalAidDeductionService;

    public MedicalAidDeductionController(IMedicalAidDeductionService medicalAidDeductionService)
    {
      _medicalAidDeductionService = medicalAidDeductionService;
    }

    [HttpGet("all")]
    [Authorize(Roles = "SuperUser")]
    public Task<IActionResult> GetAllMedicalDeductions()
    {
      throw new NotImplementedException();
    }

    [HttpGet("employee/{id}")]
    [Authorize(Roles = "SuperUser")]
    public Task<IActionResult> GetEmployeeMedicalAidDeductionById([FromRoute] string id)
    {
      throw new NotImplementedException();
    }

    [HttpPost("create/employee/{id}")]
    [Authorize(Roles = "SuperUser")]
    public Task<IActionResult>
      CreateNewEmployeeMedicalAidDeduction([FromRoute] string id) // FromBody Param needed
    {
      throw new NotImplementedException();
    }

  }
}
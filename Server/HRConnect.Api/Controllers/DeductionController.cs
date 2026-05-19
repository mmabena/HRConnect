namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs.Payroll.Deduction;
  using HRConnect.Api.Interfaces.Payroll.Deduction;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  [Route("api/deduction")]
  [ApiController]
  [Authorize(Roles = "SuperUser")]
  public class DeductionController(IDeductionService deductionService) : ControllerBase
  {
    private readonly IDeductionService _deductionService = deductionService;

    ///<summary>
    ///Creates a new deduction in the system.
    ///</summary>
    ///<param name="deductionAddDto">Deduction add request data transfer object</param>
    ///<returns>
    ///Created deduction data transfer object
    ///</returns>
    [HttpPost]
    public async Task<IActionResult> CreateNewDeduction(DeductionAddDto deductionAddDto)
    {
      DeductionDto deductionDto = await _deductionService.AddAsync(deductionAddDto);
      return Ok(deductionDto);
    }

    ///<summary>
    ///Retrieves a list of all deductions in the system. 
    ///</summary>
    ///<returns>
    ///List of deductions
    ///</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllDeductions()
    {
      List<DeductionDto> deductionDtos = await _deductionService.GetAllDeductionsAsync();
      return Ok(deductionDtos);
    }

    ///<summary>
    ///Retrieves all deductions for a specific company from the system.
    ///</summary>
    ///<param name="companyId">Company ID</param>
    ///<returns>
    ///List of deductions for the specified company
    ///</returns>
    [HttpGet]
    [Route("company/{companyId}")]
    public async Task<IActionResult> GetDeductionsByCompanyId([FromRoute] string companyId)
    {
      List<DeductionDto> deductionDtos = await _deductionService.GetDeductionsByCompanyIdAsync(companyId);
      return Ok(deductionDtos);
    }

    ///<summary>
    ///Retrieves a deduction by its code.
    ///</summary>
    ///<param name="code">The code of the deduction</param>
    ///<returns>
    ///The deduction with the specified code
    ///</returns>
    [HttpGet]
    [Route("code/{code}")]
    public async Task<IActionResult> GetDeductionByCode([FromRoute] string code)
    {
      DeductionDto? deductionDto = await _deductionService.GetDeductionByCodeAsync(code);
      return Ok(deductionDto);
    }

    ///<summary>
    ///Updates an existing deduction in the system.
    ///</summary>
    ///<param name="deductionUpdateDto">Deduction update request data transfer object</param>
    ///<returns>
    ///Updated deduction data transfer object
    ///</returns>
    [HttpPut]
    public async Task<IActionResult> UpdateDeduction(DeductionUpdateDto deductionUpdateDto)
    {
      DeductionDto deductionDto = await _deductionService.UpdateAsync(deductionUpdateDto);
      return Ok(deductionDto);
    }

    ///<summary>
    ///Sets a deduction as inactive in the system.
    ///</summary>
    ///<param name="code">The code of the deduction to be set as inactive</param>
    ///<returns>
    ///Result of the operation
    ///</returns>
    [HttpPatch]
    [Route("inactive/{code}")]
    public async Task<IActionResult> SetDeductionInactive([FromRoute] string code)
    {
      string result = await _deductionService.DeleteAsync(code);
      return Ok(result);
    }
  }
}

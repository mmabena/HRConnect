namespace HRConnect.Api.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Http;
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Interfaces;
  using Microsoft.AspNetCore.Authorization;

  [ApiController]
  [Route("api/tax-deductions")]
  [Authorize(Roles = "SuperUser")]
  public class TaxDeductionController : ControllerBase
  {
    private readonly ITaxDeductionService _taxDeductionService;

    /// <summary>
    /// Initializes a new instance of the TaxDeductionController
    /// </summary>
    /// <param name="taxDeductionService">Tax deduction service</param>
    public TaxDeductionController(ITaxDeductionService taxDeductionService)
    {
      _taxDeductionService = taxDeductionService;
    }

    /// <summary>
    /// Calculates the tax payable based on tax year, remuneration and age
    /// </summary>
    [HttpGet("calculate")]
    public async Task<ActionResult<decimal>> CalculateTax(
      [FromQuery] decimal remuneration,
      [FromQuery] int age)
    {
      try
      {
        var tax = await _taxDeductionService
          .CalculateTaxAsync(remuneration, age);

        return Ok(tax);
      }
      catch (ArgumentException ex)
      {
        return BadRequest(ex.Message);
      }
    }
  }
}

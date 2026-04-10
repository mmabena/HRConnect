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
  using HRConnect.Api.DTOs.TaxDeduction;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Data;
  using Microsoft.EntityFrameworkCore;
  using System.Security.Claims;

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
    public async Task<ActionResult<decimal>> CalculateTax([FromQuery] decimal remuneration, [FromQuery] int age)
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

    /// <summary>
    /// Calculates tax including pension and medical credits
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<FinalTaxDeduction>> GenerateTax(
     [FromBody] TaxCalculationDto request)
    {
      try
      {
        var result = await _taxDeductionService.GenerateTaxAsync(request);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }
  }
}

namespace HRConnect.Api.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Http;
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Interfaces;

  [ApiController]
  [Route("api/tax-deductions")]
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
      [FromQuery] int taxYear,
      [FromQuery] decimal remuneration,
      [FromQuery] int age)
    {
      try
      {
        var tax = await _taxDeductionService
          .CalculateTaxAsync(taxYear, remuneration, age);

        return Ok(tax);
      }
      catch (ArgumentException ex)
      {
        return BadRequest(ex.Message);
      }
    }

    /// <summary>
    /// Uploads a tax deduction Excel document for a specific tax year.
    /// The uploaded document replaces all existing tax deductions for that year.
    /// </summary>
    /// <param name="request">Excel file and tax year</param>
    /// <returns>No content if successful</returns>
    [HttpPut("upload")]
    public async Task<IActionResult> UploadTaxTable(
     [FromForm] TaxTableUploadRequest request)
    {
      if (request.File == null || request.File.Length == 0)
      {
        return BadRequest("Excel file is required.");
      }

      
      var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
      if (extension != ".xlsx" && extension != ".xls")
      {
        return BadRequest("Invalid file type. Only .xlsx or .xls files are allowed.");
      }

      var allowedContentTypes = new[]
      {
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    "application/vnd.ms-excel"
  };

      if (!allowedContentTypes.Contains(request.File.ContentType))
      {
        return BadRequest("Invalid Excel file format.");
      }

      if (request.TaxYear < 2000 || request.TaxYear > DateTime.UtcNow.Year + 1)
      {
        return BadRequest("Invalid tax year.");
      }

      try
      {
        await _taxDeductionService
          .UploadTaxTableAsync(request.TaxYear, request.File);

        return NoContent();
      }
      catch (ArgumentException ex)
      {
        return BadRequest(ex.Message);
      }
    }

  }
}

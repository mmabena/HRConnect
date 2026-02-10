namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Interfaces;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Mvc;
  using System;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authorization;
  
  /// <summary>
  /// Handles tax table retrieval and upload operations
  /// This controller allows administrators to upload annual tax tables
  /// and retrieve tax deduction data for a specific tax year
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(Roles = "SuperUser")]
  public class TaxTableUploadController : ControllerBase
  {
    private readonly ITaxDeductionService _taxDeductionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaxTableUploadController"/>.
    /// </summary>
    /// <param name="taxDeductionService">
    /// Service responsible for tax deduction business logic.
    /// </param>
    public TaxTableUploadController(ITaxDeductionService taxDeductionService)
    {
      _taxDeductionService = taxDeductionService;
    }

    /// <summary>
    /// Retrieves all tax deduction brackets for a given tax year.
    /// </summary>
    /// <param name="taxYear">
    /// The tax year for which tax deductions should be retrieved.
    /// </param>
    /// <returns>
    /// A list of tax deduction records for the specified tax year.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(int taxYear)
    {
      var allDeductions = await _taxDeductionService.GetAllTaxDeductionsAsync(taxYear);
      return Ok(allDeductions);
    }

    /// <summary>
    /// Uploads an Excel tax table for a specific tax year.
    /// Only one active tax table per year is allowed.
    /// </summary>
    /// <param name="request">
    /// The upload request containing the tax year and Excel file.
    /// </param>
    /// <returns>
    /// No content if the upload is successful.
    /// </returns>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadTaxTable([FromForm] TaxTableUploadRequest request)
    {
      // Validations
      if (request.File == null || request.File.Length == 0)
        return BadRequest("Excel file is required.");

      var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
      if (extension != ".xlsx" && extension != ".xls")
        return BadRequest("Invalid file type. Only .xlsx or .xls files are allowed.");

      var allowedContentTypes = new[]
      {
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-excel"
    };

      if (!allowedContentTypes.Contains(request.File.ContentType))
        return BadRequest("Invalid Excel file format.");

      if (request.TaxYear < 2000 || request.TaxYear > DateTime.UtcNow.Year + 1)
        return BadRequest("Invalid tax year.");

      try
      {
        await _taxDeductionService.UploadTaxTableAsync(request.TaxYear, request.File);
        return NoContent();
      }
      catch (ArgumentException ex)
      {
        return BadRequest(ex.Message);
      }
    }
  }
}

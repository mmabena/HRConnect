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
    private readonly ITaxTableUploadService _taxTableUploadService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaxTableUploadController"/>.
    /// </summary>
    /// <param name="taxTableUploadService">
    /// Service responsible for tax table upload business logic.
    /// </param>
    public TaxTableUploadController(ITaxTableUploadService taxTableUploadService)
    {
      _taxTableUploadService = taxTableUploadService;
    }

    /// <summary>
    /// Retrieves all tax table uploads.
    /// </summary>
    /// <param name="taxYear">
    /// The tax year for which tax deductions should be retrieved.
    /// </param>
    /// <returns>
    /// A list of tax table upload records.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(int taxYear)
    {
      var allDeductions = await _taxTableUploadService.GetAllUploadsAsync();
      return Ok(allDeductions);
    }

    /// <summary>
    /// Uploads an Excel tax table for a specific tax year.
    /// Taax Tables will only be made active on the 1st of March.
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
      if (request.File == null || request.File.Length == 0)
        return BadRequest("Excel file is required.");

      try
      {
        var result = await _taxTableUploadService
            .UploadTaxTableAsync(request.TaxYear, request.File);

        return Ok(result);
      }
      catch (ArgumentException ex)
      {
        return BadRequest(ex.Message);
      }
      catch (InvalidOperationException ex)
      {
        return BadRequest(ex.Message);
      }
      catch (Exception)
      {
        return StatusCode(StatusCodes.Status500InternalServerError,
            "An unexpected error occurred while uploading the tax table.");
      }
    }
  }
}

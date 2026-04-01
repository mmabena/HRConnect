namespace HRConnect.Api.Controllers;

using DTOs;
using DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API controller for managing medical aid deductions for employees.
/// Provides endpoints for creating, retrieving, updating, and terminating medical aid deductions.
/// Requires SuperUser role authorization for all operations.
/// </summary>
[Route("api/medical-aid-deductions")]
[ApiController]
public class MedicalAidDeductionController : ControllerBase
{
    private readonly IMedicalAidDeductionService _medicalAidDeductionService;
    private readonly IMedicalAidEligibilityService _eligibilityService;

    /// <summary>
    /// Initializes a new instance of the MedicalAidDeductionController.
    /// </summary>
    /// <param name="medicalAidDeductionService">The service for medical aid deduction operations.</param>
    /// <param name="eligibilityService">The service for medical aid eligibility operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when any service parameter is null.</exception>
    public MedicalAidDeductionController(
        IMedicalAidDeductionService medicalAidDeductionService,
        IMedicalAidEligibilityService eligibilityService)
    {
        _medicalAidDeductionService = medicalAidDeductionService;
        _eligibilityService = eligibilityService;
    }

    /// <summary>
    /// Retrieves all medical aid deductions from finalized payroll runs.
    /// </summary>
    /// <returns>A collection of all medical aid deductions.</returns>
    /// <response code="200">Returns all medical aid deductions successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="500">Internal server error.</response>
    /// <remarks>
    /// This endpoint retrieves deductions from finalized and locked payroll runs only.
    /// Currently authorization is commented out for testing purposes.
    /// </remarks>
    [HttpGet("all")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetAllMedicalDeductions()
    {
      var deductions = await _medicalAidDeductionService.GetAllMedicalAidDeductions();
      return Ok(deductions);
    }

    /// <summary>
    /// Retrieves medical aid deductions for a specific employee from active payroll runs.
    /// </summary>
    /// <param name="id">The unique identifier of the employee.</param>
    /// <returns>A collection of medical aid deductions for the specified employee.</returns>
    /// <response code="200">Returns employee deductions successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="500">Internal server error.</response>
    /// <remarks>
    /// Only retrieves deductions from non-finalized, non-locked payroll runs.
    /// </remarks>
    [HttpGet("employee/{id}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetEmployeeMedicalAidDeductionById([FromRoute] string id)
    {
      var deduction = await _medicalAidDeductionService.GetMedicalAidDeductionsByEmployeeIdAsync(id);
      return Ok(deduction);
    }

    /// <summary>
    /// Retrieves eligible medical options for an employee based on their salary and dependents.
    /// This is the first step before creating a deduction.
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="request">Dependent counts (principals, adults, children)</param>
    /// <returns>List of eligible medical options with calculated premiums</returns>
    /// <response code="200">Returns eligible options successfully.</response>
    /// <response code="400">Request body is required or invalid.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="404">Employee not found.</response>
    /// <response code="500">Internal server error.</response>
    /// <remarks>
    /// Use this endpoint to show employees what medical options they qualify for
    /// before proceeding with deduction creation.
    /// </remarks>
    [HttpPost("employee/{id}/eligible-options")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetEligibleMedicalOptions(
      [FromRoute] string id,
      [FromBody] RequestEligibileOptionsDto request)
    {
      if (request == null)
      {
        return BadRequest("Request body is required");
      }

      var eligibleOptions = await _eligibilityService.GetEligibleMedicalOptionsForEmployeeAsync(id, request);
      return Ok(eligibleOptions);
    }

    /// <summary>
    /// Creates a new medical aid deduction for an employee.
    /// Call this after the user has selected an option from the eligible options endpoint.
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="request">Selected medical option details and premiums</param>
    /// <returns>The created medical aid deduction</returns>
    /// <response code="200">Medical aid deduction created successfully.</response>
    /// <response code="400">Request body is required or MedicalOptionId is invalid.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="404">Employee not found or medical option not eligible.</response>
    /// <response code="500">Internal server error.</response>
    /// <remarks>
    /// This should be called after the employee has selected a specific medical option
    /// from the eligible options returned by the GetEligibleMedicalOptions endpoint.
    /// The deduction will be created in the current active payroll run.
    /// </remarks>
    [HttpPost("create/employee/{id}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> CreateNewEmployeeMedicalAidDeduction(
      [FromRoute] string id,
      [FromBody] CreateMedicalAidDeductionRequestDto request)
    {
      if (request == null)
      {
        return BadRequest("Request body is required with selected medical option details");
      }

      if (request.MedicalOptionId <= 0)
      {
        return BadRequest("MedicalOptionId is required");
      }

      var deduction = await _medicalAidDeductionService.AddNewMedicalAidDeductions(
        id,
        request.MedicalOptionId,
        request);

      return Ok(deduction);
    }

    /// <summary>
    /// Updates an employee's active medical aid deduction.
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="request">Updated medical aid deduction details</param>
    /// <returns>The updated medical aid deduction</returns>
    /// <response code="200">Medical aid deduction updated successfully.</response>
    /// <response code="400">Request body is required or invalid.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="404">No active deduction found for employee.</response>
    /// <response code="500">Internal server error.</response>
    /// <remarks>
    /// Updates the currently active medical aid deduction for an employee.
    /// Only works with deductions in non-finalized, non-locked payroll runs.
    /// </remarks>
    [HttpPut("employee/{id}/update-deductions")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> UpdateEmployeeActiveMedicalAidDeductions([FromRoute] string id
      , [FromBody] UpdateMedicalAidDeductionRequestDto request)
    {
      var update = await _medicalAidDeductionService.UpdateDeductionsByEmpIdAsync(id, request);
      return Ok(update);
    }

    /// <summary>
    /// Terminates an employee's active medical aid deduction.
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="terminationRequest">Termination details including reason and date</param>
    /// <returns>The terminated medical aid deduction</returns>

    /// <remarks>
    /// Terminates the currently active medical aid deduction for an employee.
    /// Sets the IsActive flag to false and records termination details.
    /// Only works with deductions in non-finalized, non-locked payroll runs.
    /// </remarks>
    [HttpPatch("employee/{id}/terminate-deductions")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> TerminateEmployeeMedicalAidDeductionByEmpId(
      [FromRoute] string id, [FromBody] TerminateMedicalAidDeductionRequestDto terminationRequest)
    {
      var response =
        await _medicalAidDeductionService.TerminateDeductionsByEmpIdAsync(id, terminationRequest);

      return Ok(response);
    }
}

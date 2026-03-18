namespace HRConnect.Api.Controllers;

using DTOs;
using DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/medical-aid-deductions")]
[ApiController]
public class MedicalAidDeductionController : ControllerBase
{
    private readonly IMedicalAidDeductionService _medicalAidDeductionService;
    private readonly IMedicalAidEligibilityService _eligibilityService;

    public MedicalAidDeductionController(
        IMedicalAidDeductionService medicalAidDeductionService,
        IMedicalAidEligibilityService eligibilityService)
    {
        _medicalAidDeductionService = medicalAidDeductionService;
        _eligibilityService = eligibilityService;
    }

    /// <summary>
    /// Get all medical aid deductions (SuperUser only).
    /// </summary>
    [HttpGet("all")]
    //[Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetAllMedicalDeductions()
    {
        var deductions = await _medicalAidDeductionService.GetAllMedicalAidDeductions();
        return Ok(deductions);
    }

    /// <summary>
    /// Get medical aid deductions for a specific employee by ID.
    /// </summary>
    [HttpGet("employee/{id}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetEmployeeMedicalAidDeductionById([FromRoute] string id)
    {
        var deduction = await _medicalAidDeductionService.GetMedicalAidDeductionsByEmployeeIdAsync(id);
        return Ok(deduction);
    }

    /// <summary>
    /// Get eligible medical options for an employee based on their salary and dependents.
    /// This is the first step before creating a deduction.
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="request">Dependent counts (principals, adults, children)</param>
    /// <returns>List of eligible medical options with calculated premiums</returns>
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
    /// Create a new medical aid deduction for an employee.
    /// Call this after the user has selected an option from the eligible options endpoint.
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="request">Selected medical option details and premiums</param>
    /// <returns>The created medical aid deduction</returns>
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
        /*CreatedAtAction(
          nameof(GetEmployeeMedicalAidDeductionById),
          new {id = deduction.EmployeeId},
          deduction);*/
    }
}
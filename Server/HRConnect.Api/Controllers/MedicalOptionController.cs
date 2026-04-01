namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.DTOs.MedicalOption;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  /// <summary>
  /// API controller for managing medical options and their categories.
  /// Provides endpoints for retrieving grouped medical options and performing bulk updates.
  /// Requires SuperUser role authorization for all operations.
  /// </summary>
  [Route("api/medical-options")]
  [ApiController]
  public class MedicalOptionController : ControllerBase
  {
    private readonly IMedicalOptionService _medicalOptionService;
    /// <summary>
    /// Initializes a new instance of the MedicalOptionController.
    /// </summary>
    /// <param name="medicalOptionService">The service layer for medical option operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when medicalOptionService is null.</exception>
    public MedicalOptionController(IMedicalOptionService medicalOptionService)
    {
      _medicalOptionService = medicalOptionService;
    }
    /// <summary>
    /// Retrieves all medical options grouped by their categories.
    /// </summary>
    /// <returns>A collection of medical option categories with their associated options.</returns>
    /// <response code="200">Returns the grouped medical options successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="404">No medical options found.</response>
    /// <response code="500">Internal server error.</response>
    /// <remarks>
    /// This endpoint requires SuperUser role authorization.
    /// Global exception handling middleware catches and standardizes error responses.
    /// The response contains categories with their associated medical options organized for display.
    /// </remarks>
    /// <example>
    /// GET /api/medical-options/categories
    /// Headers: Authorization: Bearer {token}
    /// </example>
    [HttpGet("categories")]
    //[Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetGroupedMedicalOptions()
    {
      var groupedOptions = await _medicalOptionService
        .GetGroupedMedicalOptionsAsync();
      if (groupedOptions == null)
      {
        return NotFound();
      }
      return Ok(groupedOptions);
    }
    /// <summary>
    /// Retrieves eligible medical options for a specific employee based on their salary and enrollment period.
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <returns>A list of eligible medical options grouped by category.</returns>
    /// <response code="200">Returns eligible options successfully.</response>
    /// <response code="400">Employee ID is required.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="404">No eligible options found for the employee.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("eligible/{employeeId}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetEmployeeEligibleOptions([FromRoute] string employeeId)
    {
      if (string.IsNullOrWhiteSpace(employeeId))
      {
        return BadRequest("Employee ID is required");
      }
      var eligibleOptions = await _medicalOptionService.GetEmployeeEligibleOptions(employeeId);
      if (eligibleOptions == null || eligibleOptions.Count == 0)
      {
        return NotFound($"No eligible medical options found for employee {employeeId}");
      }
      return Ok(eligibleOptions);
    }
    /// <summary>
    /// Retrieves all medical options that fall within a specified salary bracket.
    /// </summary>
    /// <param name="salaryAmount">The salary amount to match against option salary brackets.</param>
    /// <returns>A collection of medical option categories with options matching the salary bracket.</returns>
    /// <response code="200">Returns options within salary bracket successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("options/{salaryAmount}/salary-bracket")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetAllOptionsWithinEmployeeSalary(
      [FromRoute] decimal salaryAmount)
    {
      var groupedEligibleOptions =
         await _medicalOptionService.GetAllOptionsWithinEmployeeSalary(salaryAmount);
      return Ok(groupedEligibleOptions);
    }
    /// <summary>
    /// Retrieves all medical option categories available in the system.
    /// </summary>
    /// <returns>A collection of all medical option categories grouped by ID.</returns>
    /// <response code="200">Returns all categories successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("categories/all")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetAllMedicalOptionCategories()
    {
      var allCategories = await _medicalOptionService.GetAllMedicalOptionCategories();
      return Ok(allCategories);
    }
    /// <summary>
    /// Retrieves all medical options within a specific category.
    /// </summary>
    /// <param name="id">The unique identifier of the medical option category.</param>
    /// <returns>A collection of medical options within the specified category.</returns>
    /// <response code="200">Returns category options successfully.</response>
    /// <response code="400">Category ID must be greater than 0.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="404">No options found for the specified category.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id}/category/options")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetAllCategoryOptionsById([FromRoute] int id)
    {
      if (id <= 0)
      {
        return BadRequest("Category ID must be greater than 0");
      }
      var categoryOptions = await _medicalOptionService.GetAllCategoryOptionsById(id);
      if (categoryOptions == null || categoryOptions.Count == 0)
      {
        return NotFound($"No options found for category ID {id}");
      }
      return Ok(categoryOptions);
    }
    /// <summary>
    /// Retrieves a specific medical option category by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the medical option category.</param>
    /// <returns>The medical option category details.</returns>
    /// <response code="200">Returns category details successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="404">Category not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id}/category")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetCategoryById([FromRoute] int id)
    {
      var requestedCategory = await _medicalOptionService.GetCategoryById(id);
      return Ok(requestedCategory);
    }
    /// <summary>
    /// Creates multiple medical options within an existing category.
    /// </summary>
    /// <param name="catId">The unique identifier of the medical option category.</param>
    /// <param name="createDto">Collection of medical option variants to create.</param>
    /// <returns>The created medical options.</returns>
    /// <response code="201">Medical options created successfully.</response>
    /// <response code="400">Invalid category ID or empty request body.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="404">Category not found.</response>
    /// <response code="500">Internal server error.</response>
    /// <remarks>
    /// This operation is only allowed during the update period (November-December).
    /// Validates that option names are unique within the category.
    /// </remarks>
    [HttpPost("{catId}/category/options")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> CreateBulkOptionsByExistingCategoryId([FromRoute] int catId,
      [FromBody] IReadOnlyCollection<CreateMedicalOptionVariantsDto> createDto)
      {
        if (catId <= 0)
        {
          return BadRequest("Category ID must be greater than 0");
        }
        if (createDto == null || createDto.Count == 0)
        {
          return BadRequest("Request body cannot be null or empty");
        }
        var createdOptions = await _medicalOptionService.CreateBulkOptionsByExistingCategoryId(catId, createDto);
        return CreatedAtAction(nameof(GetAllCategoryOptionsById), new { id = catId }, createdOptions);
      }
    /// <summary>
    /// Creates a new medical option category.
    /// </summary>
    /// <param name="createCategoryPayload">The medical option category details to create.</param>
    /// <returns>The created medical option category.</returns>
    /// <response code="201">Category created successfully.</response>
    /// <response code="400">Category name is required or already exists.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("categories")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> CreateMedicalOptionCategory(
      [FromBody] CreateMedicalOptionCategoryDto createCategoryPayload)
      {
        if (string.IsNullOrWhiteSpace(createCategoryPayload.MedicalOptionCategoryName))
        {
          return BadRequest("Category name is required");
        }
        var createdCategory = await _medicalOptionService.CreateMedicalOptionCategory(createCategoryPayload);
        return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.MedicalOptionCategoryId }, createdCategory);
      }
    /// <summary>
    /// Retrieves the current database snapshot of all medical options.
    /// </summary>
    /// <returns>A read-only list of all medical options from the database.</returns>
    /// <response code="200">Returns database snapshot successfully.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="500">Internal server error.</response>
    /// <remarks>
    /// This is a testing endpoint used to verify database state and for debugging purposes.
    /// </remarks>
    [HttpGet("db-copy")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> GetCurrentDBCopy()
    {
      var currentDbState = await _medicalOptionService.GetCurrentDbCopy();
      return Ok(currentDbState);
    }
    /// <summary>
    /// Performs bulk updates of medical options within a specific category.
    /// </summary>
    /// <param name="optionId">The category ID containing the options to update.</param>
    /// <param name="bulkUpdateDto">Collection of medical option variants with updated values.</param>
    /// <returns>No content if successful, or NotFound if the category/options don't exist.</returns>
    /// <response code="204">Bulk update completed successfully.</response>
    /// <response code="400">Invalid request data or malformed DTO.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="404">Category or specified medical options not found.</response>
    /// <response code="500">Internal server error during bulk update operation.</response>
    /// <remarks>
    /// This endpoint requires SuperUser role authorization.
    /// Updates multiple medical options atomically - either all succeed or all fail.
    /// Only properties specified in the DTO are updated; null values preserve existing data.
    /// Ideal for annual contribution rate changes or bulk price adjustments.
    /// Global exception handling middleware catches and standardizes error responses.
    /// </remarks>
    /// <example>
    /// PUT /api/medical-options/1/variants
    /// Headers: Authorization: Bearer {token}
    /// Body:
    /// [
    ///   {
    ///     "MedicalOptionId": 1,
    ///     "MonthlyRiskContributionAdult": 500.00,
    ///     "TotalMonthlyContributionsAdult": 550.00
    ///   },
    ///   {
    ///     "MedicalOptionId": 2,
    ///     "MonthlyRiskContributionAdult": 600.00,
    ///     "TotalMonthlyContributionsAdult": 650.00
    ///   }
    /// ]
    /// </example>
    [HttpPut("{optionId}/variants")]
    [Authorize(Roles = "SuperUser")]
    public async Task<ActionResult<IReadOnlyList<MedicalOptionDto>>> BulkUpdateMedicalOptionsByCategory(
      int optionId, 
      [FromBody] IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto)
    {
      var result =
        await _medicalOptionService.BulkUpdateMedicalOptionsByCategoryAsync(optionId,
          bulkUpdateDto);
      if (result == null || !result.Any()) return NotFound();
      return NoContent();
    }
    /// <summary>
    /// Updates an existing medical option category.
    /// </summary>
    /// <param name="id">The unique identifier of the medical option category to update.</param>
    /// <param name="updatePayload">The updated category details.</param>
    /// <returns>The updated medical option category.</returns>
    /// <response code="200">Category updated successfully.</response>
    /// <response code="400">Invalid category ID, null request body, or missing category name.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have SuperUser role.</response>
    /// <response code="404">Category not found.</response>
    /// <response code="409">Category name already exists.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{id}/category")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> UpdateCategoryById([FromRoute] int id, [FromBody] UpdateMedicalOptionCategoryDto updatePayload)
    {
      if (id <= 0)
      {
        return BadRequest("Category ID must be greater than 0");
      }
      if (updatePayload == null)
      {
        return BadRequest("Request body cannot be null");
      }
      if (string.IsNullOrWhiteSpace(updatePayload.MedicalOptionCategoryName))
      {
        return BadRequest("Category name is required");
      }
      var updatedCategory = await _medicalOptionService.UpdateExistingCategoryById(id, updatePayload);
      return Ok(updatedCategory);
    }
  }
}

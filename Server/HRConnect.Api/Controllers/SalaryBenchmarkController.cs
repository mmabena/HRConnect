namespace HRConnect.Api.Controllers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection.Metadata;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Authorization;
  using System.Security.Claims;
  using HRConnect.Api.DTOs.Benchmarking;
  using HRConnect.Api.Interfaces;
  using System.Security.Cryptography;

  [ApiController]
  [Route("api/salary-benchmarks")]
  [Authorize(Roles = "SuperUser")]

  public class SalaryBenchmarkController : ControllerBase
  {
    private readonly ISalaryBenchmarkService _service;

    public SalaryBenchmarkController(ISalaryBenchmarkService service)
    {
      _service = service;
    }

    /// <summary>
    /// This endpoint allows the admin to create a new salary benchmark for 
    /// different job grade and locations.
    /// </summary>
    /// <param name="request">salary benchmark request</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SalaryBenchmarkRequestDto request)
    {
      ///makes sure that the values being added isnt 0 or a negative salary
      bool salaryValuesInValid =
      request.Salary25th <= 0 ||
      request.Salary50th <= 0 ||
      request.Salary75th <= 0;

      if (salaryValuesInValid)
      {
        return BadRequest("Salary values must be greater than 0.");
      }

      //percentiles validation order one should be greater than the other 
      if (request.Salary25th >= request.Salary50th)
      {
        return BadRequest("P50 must be greater than P25");
      }

      if (request.Salary50th >= request.Salary75th)
      {
        return BadRequest("P75 must be greater than P50");
      }

      if (request.Year < 2000 || request.Year > DateTime.UtcNow.Year + 1)
      {
        return BadRequest("Please provide a valid Year");
      }
      
      bool requiredFieldsMissing =
      string.IsNullOrWhiteSpace(request.Location) ||
      string.IsNullOrWhiteSpace(request.Source);
      if (requiredFieldsMissing)
      {
        return BadRequest("Location and Source are required fields.");
      }

      var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
      var result = await _service.CreateAsync(request, email);

      //if the position and location already exists
      if (result == null)
      {
        return BadRequest("A benchmark for this position has been already created");
      }
      return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    /// <summary>
    /// This endpoint allows the admin to view all salary benchmarks entered into the system,
    ///  sorted by most recent.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var benchmarks = await _service.GetAllAsync();
      return Ok(benchmarks);
    }

    /// <summary>
    /// This endpoint allows the admin to update an existing salary benchmark.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SalaryBenchmarkUpdateDto request)
    {
      if (id <= 0)
      {
        return BadRequest("Please enter a valid Benchmark Id");
      }
      if (request.Salary25th <= 0 || request.Salary50th <= 0 || request.Salary75th <= 0)
      {
        return BadRequest("Salary percentile has to be greater than 0");
      }

      if (request.Salary25th >= request.Salary50th)
      {
        return BadRequest("P50 must be greater than P25");
      }

      if (request.Salary50th >= request.Salary75th)
      {
        return BadRequest("P75 must be greater than P50");
      }

      if (string.IsNullOrWhiteSpace(request.Source))
      {
        return BadRequest("Source can not be left empty");
      }

      var result = await _service.UpdateAsync(id, request);

      if (result == null)
      {
        return NotFound($"Salary benchmark with id {id} was not found.");
      }

      return Ok(result);
    }

    /// <summary>
    /// This endpoint allows the admin to view all the employees
    /// with their salary benchmark data.
    /// </summary>
    /// <returns></returns>
    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployeeSalaryBenchmarksAsync()
    {
      var result = await _service.GetEmployeeSalaryBenchmarksAsync();
      return Ok(result);
    }

    /// <summary>
    /// Views the summary of the salary benchmarks
    /// </summary>
    /// <returns></returns>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
      var result = await _service.GetSummaryAsync();
      return Ok(result);
    }
  }
}
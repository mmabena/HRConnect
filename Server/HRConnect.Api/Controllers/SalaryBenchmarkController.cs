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

      bool requiredFieldsMissing =
      string.IsNullOrWhiteSpace(request.Location) ||
      string.IsNullOrWhiteSpace(request.Source);
      if (requiredFieldsMissing)
      {
        return BadRequest("Location and Source are required fields.");
      }
      var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;

      var createdBenchmark = await _service.CreateAsync(request, email);
      return CreatedAtAction(nameof(GetAll), new { id = createdBenchmark.Id }, createdBenchmark);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
      var benchmarks = await _service.GetAllAsync();
      return Ok(benchmarks);
    }

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
  }
}
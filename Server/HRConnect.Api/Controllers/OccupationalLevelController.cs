namespace HRConnect.Api.Controllers
{
  using System.Collections.Generic;
  using Microsoft.AspNetCore.Authorization;
  using System.Runtime.ExceptionServices;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs.OccupationalLevel;
  using HRConnect.Api.Interfaces;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  [Route("api/[controller]")]
  [Authorize(Roles = "SuperUser")] // Require authentication and SuperAdmin role
 
  public class OccupationalLevelController : ControllerBase
  {
    private readonly IOccupationalLevelService _occupationalLevelService;

    public OccupationalLevelController(IOccupationalLevelService occupationalLevelService)
    {
      _occupationalLevelService = occupationalLevelService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OccupationalLevelDto>>> GetAllOccupationalLevels()
    {
      var occupationalLevels = await _occupationalLevelService.GetAllOccupationalLevelsAsync();
      return Ok(occupationalLevels);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OccupationalLevelDto>> GetOccupationalLevelById(int id)
    {
      var occupationalLevel = await _occupationalLevelService.GetOccupationalLevelByIdAsync(id);
      if (occupationalLevel == null)
        return NotFound();

      return Ok(occupationalLevel);
    }

    [HttpPost]
    public async Task<ActionResult<OccupationalLevelDto>> CreateOccupationalLevel([FromBody] CreateOccupationalLevelDto createOccupationalLevelDto)
    {
      
        var createdOccupationalLevel = await _occupationalLevelService.AddOccupationalLevelAsync(createOccupationalLevelDto);
        return CreatedAtAction(
            nameof(GetAllOccupationalLevels), 
            new { id = createdOccupationalLevel.OccupationalLevelId }, 
            createdOccupationalLevel);
      
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<OccupationalLevelDto>> UpdateOccupationalLevel(int id, [FromBody] UpdateOccupationalLevelDto updateOccupationalLevelDto)
    {
      var updatedOccupationalLevel = await _occupationalLevelService.UpdateOccupationalLevelAsync(id, updateOccupationalLevelDto);

      if (updatedOccupationalLevel == null)
        return NotFound();

      return Ok(updatedOccupationalLevel);
    }
  }
}
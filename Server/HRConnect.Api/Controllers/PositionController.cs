namespace HRConnect.Api.Controllers
{

    using HRConnect.Api.DTOs.Position;
using HRConnect.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

    [Route("api/position")]
    [ApiController]
    [Authorize] // Require authentication
    public class PositionController : ControllerBase
    {
    private readonly IPositionService _positionService;

    public PositionController(IPositionService positionService)
    {
    _positionService = positionService;
    }

        [HttpGet]
        public async Task<IActionResult> GetAllPositions()
        {
            var positions = await _positionService.GetAllPositionsAsync();
            return Ok(positions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPositionById(int id)
        {
            var position = await _positionService.GetPositionByIdAsync(id);
            if (position == null) return NotFound();
            return Ok(position);
        }

        [HttpGet("title/{title}")]
        public async Task<IActionResult> GetPositionByTitle(string title)
        {
            var position = await _positionService.GetPositionByTitleAsync(title);
            if (position == null) return NotFound();
            return Ok(position);
        }

        [HttpPost("Create")]
        [Authorize(Roles = "Admin")] // Only Admin can create
        public async Task<IActionResult> CreatePosition([FromBody] CreatePositionDto dto)
        {
            var created = await _positionService.CreatePositionAsync(dto);
            return CreatedAtAction(nameof(GetPositionById), new { id = created.PositionId }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can update
        public async Task<IActionResult> UpdatePosition(int id, [FromBody] UpdatePositionDto dto)
        {
            var updated = await _positionService.UpdatePositionAsync(id, dto);
            if (updated == null) return NotFound();
            return NoContent();
        }
    }
}

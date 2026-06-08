namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs.JobGrade;
  using HRConnect.Api.Interfaces;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  [Route("api/jobgrades")]
  [Authorize(Roles = "SuperUser")]

  public class JobGradeController : ControllerBase
  {
    private readonly IJobGradeService _jobGradeService;

    public JobGradeController(IJobGradeService jobGradeService)
    {
      _jobGradeService = jobGradeService;
    }

    [HttpGet]
    public async Task<List<JobGradeDto>> GetAllJobGrades()
    {
      var jobGrades = await _jobGradeService.GetAllJobGradesAsync();
      return jobGrades;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobGradeDto>> GetJobGradeById(int id)
    {
      var jobGrade = await _jobGradeService.GetJobGradeByIdAsync(id);
      if (jobGrade == null)
      {
        return NotFound();
      }
      return Ok(jobGrade);
    }

    [HttpPost]
    public async Task<ActionResult<JobGradeDto>> CreateJobGrade([FromBody] CreateJobGradeDto createJobGradeDto)
    {
      var createdJobGrade = await _jobGradeService.AddJobGradeAsync(createJobGradeDto);

      return CreatedAtAction(
          nameof(GetAllJobGrades),
          new { id = createdJobGrade.JobGradeId },
          createdJobGrade);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<JobGradeDto>> UpdateJobGrade(int id, [FromBody] UpdateJobGradeDto updateJobGradeDto)
    {
      var updatedJobGrade = await _jobGradeService.EditJobGradeAsync(id, updateJobGradeDto);

      if (updatedJobGrade == null)
        return NotFound();

      return Ok(updatedJobGrade);
    }
  }
}
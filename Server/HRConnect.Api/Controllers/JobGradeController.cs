namespace HRConnect.Api.Controllers
{
    using HRConnect.Api.DTOs.JobGrade;
    using HRConnect.Api.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperUser")] // Require authentication and SuperAdmin role

    public class JobGradeController : ControllerBase
    {
        private readonly IJobGradeService _jobGradeService;

        public JobGradeController(IJobGradeService jobGradeService)
        {
            _jobGradeService = jobGradeService;
        }

        // GET /api/jobgrade
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

        // POST /api/jobgrade
        [HttpPost]
        public async Task<ActionResult<JobGradeDto>> CreateJobGrade([FromBody] CreateJobGradeDto createJobGradeDto)
        {
            var createdJobGrade = await _jobGradeService.AddJobGradeAsync(createJobGradeDto);

            return CreatedAtAction(
                nameof(GetAllJobGrades),
                new { id = createdJobGrade.JobGradeId },
                createdJobGrade);
        }

        // PUT /api/jobgrade/{id}
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

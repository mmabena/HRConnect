namespace HRConnect.Api.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Models;
    [ApiController]
    [Route("api/job-grade-groups")]
    public class JobGradeGroupController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public JobGradeGroupController(ApplicationDBContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await _context.JobGradeGroupMaps
                .Include(x => x.JobGrade)
                .GroupBy(x => x.GroupKey)
                .Select(g => new JobGradeGroupResponse
                {
                    GroupKey = g.Key,
                    JobGrades = g.Select(x => new JobGradeDto
                    {
                        JobGradeId = x.JobGradeId,
                        Name = x.JobGrade.Name
                    }).ToList()
                })
                .ToListAsync();

            return Ok(groups);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup(CreateGroupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GroupKey))
                return BadRequest("GroupKey is required");

            if (request.JobGradeIds.Count == 0)
                return BadRequest("Group must have at least one JobGrade");

            var groupExists = await _context.JobGradeGroupMaps
                .AnyAsync(x => x.GroupKey == request.GroupKey);

            if (groupExists)
                return BadRequest("GroupKey already exists");

            var validJobGrades = await _context.JobGrades
                .Where(j => request.JobGradeIds.Contains(j.JobGradeId))
                .Select(j => j.JobGradeId)
                .ToListAsync();

            if (validJobGrades.Count != request.JobGradeIds.Count)
                return BadRequest("One or more JobGrades do not exist");

            var alreadyAssigned = await _context.JobGradeGroupMaps
                .Where(x => request.JobGradeIds.Contains(x.JobGradeId))
                .Select(x => x.JobGradeId)
                .ToListAsync();

            if (alreadyAssigned.Count > 0)
                return BadRequest($"JobGrades already assigned: {string.Join(", ", alreadyAssigned)}");

            var maps = request.JobGradeIds.Select(id => new JobGradeGroupMap
            {
                JobGradeId = id,
                GroupKey = request.GroupKey
            });

            await _context.JobGradeGroupMaps.AddRangeAsync(maps);
            await _context.SaveChangesAsync();

            return Ok("Group created successfully");
        }

        [HttpPut("move")]
        public async Task<IActionResult> MoveJobGrade(MoveJobGradeRequest request)
        {
            var mapping = await _context.JobGradeGroupMaps
                .FirstOrDefaultAsync(x => x.JobGradeId == request.JobGradeId);

            if (mapping == null)
                return NotFound("Mapping not found");

            var targetGroupExists = await _context.JobGradeGroupMaps
                .AnyAsync(x => x.GroupKey == request.NewGroupKey);

            if (!targetGroupExists)
                return BadRequest("Target group does not exist");

            var currentGroupCount = await _context.JobGradeGroupMaps
                .CountAsync(x => x.GroupKey == mapping.GroupKey);

            if (currentGroupCount <= 1)
                return BadRequest("Cannot move last JobGrade from a group");

            mapping.GroupKey = request.NewGroupKey;

            await _context.SaveChangesAsync();

            return Ok("JobGrade moved successfully");
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveJobGrade(RemoveJobGradeRequest request)
        {
            var mapping = await _context.JobGradeGroupMaps
                .FirstOrDefaultAsync(x => x.JobGradeId == request.JobGradeId);

            if (mapping == null)
                return NotFound("Mapping not found");

            var groupCount = await _context.JobGradeGroupMaps
                .CountAsync(x => x.GroupKey == mapping.GroupKey);

            if (groupCount <= 1)
                return BadRequest("Cannot remove last JobGrade from a group");

            _context.JobGradeGroupMaps.Remove(mapping);
            await _context.SaveChangesAsync();

            return Ok("JobGrade removed successfully");
        }
    }
}
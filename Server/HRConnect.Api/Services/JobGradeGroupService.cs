namespace HRConnect.Api.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;

    public class JobGradeGroupService : IJobGradeGroupService
    {
        private readonly ApplicationDBContext _context;

        public JobGradeGroupService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<JobGradeGroupResponse>> GetGroupsAsync()
        {
            return await _context.JobGradeGroupMaps
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
        }

        public async Task CreateGroupAsync(CreateGroupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GroupKey))
                throw new InvalidOperationException("GroupKey is required");

            if (request.JobGradeIds.Count == 0)
                throw new InvalidOperationException(
                    "Group must have at least one JobGrade");

            var groupExists = await _context.JobGradeGroupMaps
                .AnyAsync(x => x.GroupKey == request.GroupKey);

            if (groupExists)
                throw new InvalidOperationException(
                    "GroupKey already exists");

            var validJobGrades = await _context.JobGrades
                .Where(j => request.JobGradeIds.Contains(j.JobGradeId))
                .Select(j => j.JobGradeId)
                .ToListAsync();

            if (validJobGrades.Count != request.JobGradeIds.Count)
                throw new InvalidOperationException(
                    "One or more JobGrades do not exist");

            var alreadyAssigned = await _context.JobGradeGroupMaps
                .Where(x => request.JobGradeIds.Contains(x.JobGradeId))
                .Select(x => x.JobGradeId)
                .ToListAsync();

            if (alreadyAssigned.Count > 0)
            {
                throw new InvalidOperationException(
                    $"JobGrades already assigned: {string.Join(", ", alreadyAssigned)}");
            }

            var maps = request.JobGradeIds.Select(id =>
                new JobGradeGroupMap
                {
                    JobGradeId = id,
                    GroupKey = request.GroupKey
                });

            await _context.JobGradeGroupMaps.AddRangeAsync(maps);

            await _context.SaveChangesAsync();
        }

        public async Task MoveJobGradeAsync(
            MoveJobGradeRequest request)
        {
            var mapping = await _context.JobGradeGroupMaps
                .FirstOrDefaultAsync(x =>
                    x.JobGradeId == request.JobGradeId);

            if (mapping == null)
                throw new KeyNotFoundException(
                    "Mapping not found");

            var targetGroupExists =
                await _context.JobGradeGroupMaps
                    .AnyAsync(x =>
                        x.GroupKey == request.NewGroupKey);

            if (!targetGroupExists)
            {
                throw new InvalidOperationException(
                    "Target group does not exist");
            }

            var currentGroupCount =
                await _context.JobGradeGroupMaps
                    .CountAsync(x =>
                        x.GroupKey == mapping.GroupKey);

            if (currentGroupCount <= 1)
            {
                throw new InvalidOperationException(
                    "Cannot move last JobGrade from a group");
            }

            mapping.GroupKey = request.NewGroupKey;

            await _context.SaveChangesAsync();
        }
        public async Task RemoveJobGradeAsync(
            RemoveJobGradeRequest request)
        {
            var mapping = await _context.JobGradeGroupMaps
                .FirstOrDefaultAsync(x =>
                    x.JobGradeId == request.JobGradeId);

            if (mapping == null)
                throw new KeyNotFoundException(
                    "Mapping not found");
                    
            var groupCount = await _context.JobGradeGroupMaps
                .CountAsync(x =>
                    x.GroupKey == mapping.GroupKey);
            if (groupCount <= 1)
            {
                throw new InvalidOperationException(
                    "Cannot remove last JobGrade from a group");
            }
            _context.JobGradeGroupMaps.Remove(mapping);
            await _context.SaveChangesAsync();
        }
    }
}
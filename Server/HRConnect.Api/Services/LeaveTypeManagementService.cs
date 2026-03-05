namespace HRConnect.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Models;
    using HRConnect.Api.Data;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Interfaces;

    public class LeaveTypeManagementService : ILeaveTypeManagementService
    {
        private readonly ApplicationDBContext _context;

        public LeaveTypeManagementService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<LeaveTypeResponse>> GetLeaveTypesAsync()
        {
            var leaveTypes = await _context.LeaveTypes
                .Include(l => l.EntitlementRules)
                .ToListAsync();

            return leaveTypes.Select(MapToResponse).ToList();
        }

        public async Task<LeaveTypeResponse> GetLeaveTypeByIdAsync(int id)
        {
            var leaveType = await _context.LeaveTypes
                .Include(l => l.EntitlementRules)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leaveType == null)
                throw new InvalidOperationException("Leave type not found.");

            return MapToResponse(leaveType);
        }

        public async Task<LeaveTypeResponse> CreateLeaveTypeAsync(CreateLeaveTypeRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add("Leave type name is required.");

            if (string.IsNullOrWhiteSpace(request.Code))
                errors.Add("Leave type code is required.");

            if (request.Rules.Count == 0)
                errors.Add("At least one entitlement rule must be defined.");

            var existingNames = await _context.LeaveTypes
                .Select(x => x.Name)
                .ToListAsync();

            if (existingNames.Any(x => string.Equals(x, request.Name, StringComparison.OrdinalIgnoreCase)))
                errors.Add($"Leave type name '{request.Name}' already exists.");

            var existingCodes = await _context.LeaveTypes
                .Select(x => x.Code)
                .ToListAsync();

            if (existingCodes.Any(x => string.Equals(x, request.Code, StringComparison.OrdinalIgnoreCase)))
                errors.Add($"Leave type code '{request.Code}' already exists.");

            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join(" | ", errors));

            ValidateRules(request.Rules);

            var leaveType = new LeaveType
            {
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                FemaleOnly = request.FemaleOnly,
                IsActive = true
            };

            await _context.LeaveTypes.AddAsync(leaveType);
            await _context.SaveChangesAsync();

            foreach (var rule in request.Rules)
            {
                await _context.LeaveEntitlementRules.AddAsync(new LeaveEntitlementRule
                {
                    LeaveTypeId = leaveType.Id,
                    JobGradeId = rule.JobGradeId,
                    MinYearsService = rule.MinYearsService,
                    MaxYearsService = rule.MaxYearsService,
                    DaysAllocated = rule.DaysAllocated,
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync();

            return await GetLeaveTypeByIdAsync(leaveType.Id);
        }

        public async Task<LeaveTypeResponse> UpdateLeaveTypeAsync(int id, UpdateLeaveTypeRequest request)
        {
            var leaveType = await _context.LeaveTypes
                .Include(l => l.EntitlementRules)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leaveType == null)
                throw new InvalidOperationException("Leave type not found.");

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add("Leave type name is required.");

            if (request.Rules.Count == 0)
                errors.Add("At least one entitlement rule must be defined.");

            var existingNames = await _context.LeaveTypes
                .Where(x => x.Id != id)
                .Select(x => x.Name)
                .ToListAsync();

            if (existingNames.Any(x => string.Equals(x, request.Name, StringComparison.OrdinalIgnoreCase)))
                errors.Add($"Leave type name '{request.Name}' already exists.");

            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join(" | ", errors));

            ValidateRules(request.Rules);

            leaveType.Name = request.Name;
            leaveType.Description = request.Description;
            leaveType.FemaleOnly = request.FemaleOnly;

            _context.LeaveEntitlementRules.RemoveRange(leaveType.EntitlementRules);

            foreach (var rule in request.Rules)
            {
                await _context.LeaveEntitlementRules.AddAsync(new LeaveEntitlementRule
                {
                    LeaveTypeId = leaveType.Id,
                    JobGradeId = rule.JobGradeId,
                    MinYearsService = rule.MinYearsService,
                    MaxYearsService = rule.MaxYearsService,
                    DaysAllocated = rule.DaysAllocated,
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync();

            return await GetLeaveTypeByIdAsync(leaveType.Id);
        }

        public static void ValidateRules(List<LeaveEntitlementRuleRequest> rules)
        {
            var errors = new List<string>();

            foreach (var rule in rules)
            {
                if (rule.MinYearsService < 0)
                    errors.Add($"MinYearsService cannot be negative for JobGrade {rule.JobGradeId}.");

                if (rule.MaxYearsService.HasValue &&
                    rule.MaxYearsService.Value < rule.MinYearsService)
                    errors.Add($"MaxYearsService cannot be less than MinYearsService for JobGrade {rule.JobGradeId}.");

                if (rule.DaysAllocated <= 0)
                    errors.Add($"DaysAllocated must be greater than zero for JobGrade {rule.JobGradeId}.");
            }

            var grouped = rules.GroupBy(r => r.JobGradeId);

            foreach (var group in grouped)
            {
                var ordered = group
                    .OrderBy(r => r.MinYearsService)
                    .ToList();

                for (int i = 0; i < ordered.Count - 1; i++)
                {
                    var current = ordered[i];
                    var next = ordered[i + 1];

                    if (!current.MaxYearsService.HasValue)
                    {
                        errors.Add($"Rule for JobGrade {group.Key} cannot have unlimited MaxYearsService when additional rules exist.");
                        continue;
                    }

                    if (next.MinYearsService <= current.MaxYearsService.Value)
                    {
                        errors.Add($"Overlapping service ranges detected for JobGrade {group.Key}.");
                    }

                    if (next.MinYearsService > current.MaxYearsService.Value + 0.01m)
                    {
                        errors.Add($"Gap detected in service ranges for JobGrade {group.Key}. Ranges must be continuous.");
                    }
                }
            }

            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join(" | ", errors));
        }
        private static LeaveTypeResponse MapToResponse(LeaveType l)
        {
            return new LeaveTypeResponse
            {
                Id = l.Id,
                Name = l.Name,
                Code = l.Code,
                FemaleOnly = l.FemaleOnly,
                IsActive = l.IsActive,
                Rules = l.EntitlementRules.Select(r => new LeaveEntitlementRuleSummary
                {
                    JobGradeId = r.JobGradeId,
                    MinYearsService = r.MinYearsService,
                    MaxYearsService = r.MaxYearsService,
                    DaysAllocated = r.DaysAllocated
                }).ToList()
            };
        }
    }
}
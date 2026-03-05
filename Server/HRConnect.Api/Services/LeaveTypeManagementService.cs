
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
                throw new InvalidOperationException("LeaveType not found");

            return MapToResponse(leaveType);
        }
        public async Task<LeaveTypeResponse> CreateLeaveTypeAsync(CreateLeaveTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Leave type name required.");

            if (request.Rules.Count == 0)
                throw new InvalidOperationException("At least one entitlement rule is required.");

            var exists = await _context.LeaveTypes
                .AnyAsync(x =>
                    string.Equals(x.Name, request.Name, StringComparison.OrdinalIgnoreCase));

            if (exists)
                throw new InvalidOperationException("Leave type name must be unique.");

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

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Leave type name is required.");

            if (request.Rules.Count == 0)
                throw new InvalidOperationException("At least one entitlement rule is required.");

            var exists = await _context.LeaveTypes
                .AnyAsync(x => x.Id != id &&
                               string.Equals(x.Name, request.Name, StringComparison.OrdinalIgnoreCase));

            if (exists)
                throw new InvalidOperationException("Leave type name must be unique.");

            ValidateRules(request.Rules);

            // Update editable fields only
            leaveType.Name = request.Name;
            leaveType.Description = request.Description;
            leaveType.FemaleOnly = request.FemaleOnly;

            // Remove existing rules
            _context.LeaveEntitlementRules.RemoveRange(leaveType.EntitlementRules);

            // Add updated rules
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
            foreach (var rule in rules)
            {
                if (rule.MinYearsService < 0)
                    throw new InvalidOperationException("MinYearsService cannot be negative.");

                if (rule.MaxYearsService.HasValue && rule.MaxYearsService.Value < rule.MinYearsService)
                    throw new InvalidOperationException("MaxYearsService cannot be less than MinYearsService.");

                if (rule.DaysAllocated < 0)
                    throw new InvalidOperationException("DaysAllocated cannot be negative.");
            }
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
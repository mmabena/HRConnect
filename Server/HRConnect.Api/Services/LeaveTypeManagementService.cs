namespace HRConnect.Api.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Models;
  using HRConnect.Api.Data;
  using Microsoft.EntityFrameworkCore;
  using System.Threading.Tasks;
  using HRConnect.Api.Interfaces;

  public class LeaveTypeManagementService : ILeaveTypeManagementService
  {
    private readonly ApplicationDBContext _context;
    private readonly ILeaveBalanceService _leaveBalanceService;
    private readonly ILeaveTypeManagementRepository _leaveTypeManagementRepo;
    public LeaveTypeManagementService(ApplicationDBContext context, ILeaveBalanceService leaveBalanceService,
        ILeaveTypeManagementRepository leaveTypeManagementRepo)
    {
      _context = context;
      _leaveBalanceService = leaveBalanceService;
      _leaveTypeManagementRepo = leaveTypeManagementRepo;
    }

    public async Task<EmployeeWithLeaveDto?> GetEmployeeWithLeaveByIdAsync(string employeeId)
    {
      var emp = await _leaveTypeManagementRepo.GetEmployeeWithLeaveByIdAsync(employeeId);
      if (emp != null)
      {
        return emp;
      }
      return null;
    }

    public async Task<List<EmployeeWithLeaveDto>> GetAllEmployeesWithLeaveAsync()
    {
      return await _leaveTypeManagementRepo.GetAllEmployeesWithLeaveAsync();
    }

    public async Task<List<LeaveTypeResponseDto>> GetLeaveTypesAsync()
    {
      var leaveTypes = await _leaveTypeManagementRepo.GetLeaveTypesAsync();
      return leaveTypes;
    }

    /// <summary>
    /// Retrieves a specific leave type by its unique identifier from the database, including its associated entitlement rules,
    /// maps the data to a LeaveTypeResponse DTO, and returns it to the caller, allowing for the display or further processing of the specific leave type information in the application,
    /// while throwing an InvalidOperationException if the leave type with the specified ID is not found in the database.
    /// </summary>
    /// <param name="id"></param>
    public async Task<LeaveTypeResponseDto> GetLeaveTypeByIdAsync(int id)
    {
      var leaveType = await _leaveTypeManagementRepo.GetLeaveTypeByIdAsync(id);
      if (leaveType == null)
        throw new KeyNotFoundException("Leave type not found.");

      return leaveType;
    }

    /// <summary>
    /// Creates a new leave type in the database based on the provided CreateLeaveTypeRequest DTO,
    /// including validation of the request data, checking for duplicate names and codes, 
    /// validating the entitlement rules, and then saving the new leave type along with its associated entitlement rules to the database,
    /// and finally returning the created leave type as a LeaveTypeResponse DTO to the caller, 
    /// while throwing an InvalidOperationException if any validation errors occur during the process.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<LeaveTypeResponseDto> CreateLeaveTypeAsync(CreateLeaveTypeRequestDto request)
    {
      var errors = new List<string>();

      if (string.IsNullOrWhiteSpace(request.Name))
        errors.Add("Leave type name is required.");

      if (string.IsNullOrWhiteSpace(request.Code))
        errors.Add("Leave type code is required.");

      if (request.Rules.Count == 0)
        errors.Add("At least one entitlement rule must be defined.");

      var existingNames = await _leaveTypeManagementRepo.GetExistingNames(request.Name);

      if (existingNames != null)
      {
        if (existingNames.Any(x => string.Equals(x, request.Name, StringComparison.OrdinalIgnoreCase)))
          errors.Add($"Leave type name '{request.Name}' already exists.");
      }
      var existingCodes = await _leaveTypeManagementRepo.GetExistingCodes(request.Code);

      if (existingCodes != null)
      {
        if (existingCodes.Any(x => string.Equals(x, request.Code, StringComparison.OrdinalIgnoreCase)))
          errors.Add($"Leave type code '{request.Code}' already exists.");
      }
      if (errors.Count > 0)
        throw new InvalidOperationException(string.Join(" | ", errors));

      ValidateRules(request.Rules);

      var leaveType = new LeaveType
      {
        Name = request.Name,
        Code = request.Code,
        Description = request.Description!,
        FemaleOnly = request.FemaleOnly,
        IsActive = true
      };
      //Touching the DB to create an entry
      var newLeaveTypes = await _leaveTypeManagementRepo.CreateLeaveTypeAsync(leaveType);

      var rules = request.Rules.Select(rule => new LeaveEntitlementRule
      {
        LeaveTypeId = leaveType.Id,
        JobGradeId = rule.JobGradeId,
        MinYearsService = rule.MinYearsService,
        MaxYearsService = rule.MaxYearsService,
        DaysAllocated = rule.DaysAllocated,
        IsActive = true
      }).ToList();

      leaveType.EntitlementRules = rules;

      await _leaveTypeManagementRepo.CreateLeaveEntitlementRules(rules);

      return await GetLeaveTypeByIdAsync(leaveType.Id);
      // return newLeaveTypes.ToLeaveResponseDto();
    }
    /// <summary>
    /// Updates an existing leave type in the database based on the provided UpdateLeaveTypeRequest DTO and the leave type's unique identifier,
    /// including validation of the request data, checking for duplicate names, validating the entitlement rules, 
    /// and then saving the updated leave type along with its associated entitlement rules to the database,
    /// and finally returning the updated leave type as a LeaveTypeResponse DTO to the caller,
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<LeaveTypeResponseDto> UpdateLeaveTypeAsync(int id, UpdateLeaveTypeRequestDto request)
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

      var rules = request.Rules.Select(rule => new LeaveEntitlementRule
      {
        LeaveTypeId = leaveType.Id,
        JobGradeId = rule.JobGradeId,
        MinYearsService = rule.MinYearsService,
        MaxYearsService = rule.MaxYearsService,
        DaysAllocated = rule.DaysAllocated,
        IsActive = true
      }).ToList();

      await _context.LeaveEntitlementRules.AddRangeAsync(rules);
      await _context.SaveChangesAsync();
      var employees = await _context.Employees
          .Select(e => e.EmployeeId)
          .ToListAsync();

      foreach (var empId in employees)
      {
        await _leaveBalanceService.RecalculateAnnualLeaveAsync(empId);
      }

      return await GetLeaveTypeByIdAsync(leaveType.Id);
    }
    /// <summary>
    /// Validates a list of leave entitlement rules to ensure that they meet the required criteria, 
    /// such as non-negative minimum years of service,
    /// valid maximum years of service that are not less than the minimum years, 
    /// positive days allocated, and non-overlapping service ranges for the same job grade,
    /// while throwing an InvalidOperationException with a detailed error message if any validation errors are found during the process.
    /// </summary>
    /// <param name="rules"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void ValidateRules(List<LeaveEntitlementRuleRequestDto> rules)
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

      var grouped = rules.GroupBy(r =>
          new[] { 2, 3, 4, 6 }.Contains(r.JobGradeId) ? 1 : r.JobGradeId);

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

  }
}
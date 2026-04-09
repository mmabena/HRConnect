namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Models;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Interfaces;
  using Microsoft.EntityFrameworkCore;

  public class LeaveTypeManagementRepository : ILeaveTypeManagementRepository
  {
    private readonly ApplicationDBContext _context;

    public LeaveTypeManagementRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task<LeaveType> CreateLeaveTypeAsync(LeaveType leaveType)
    {
      await _context.LeaveTypes.AddAsync(leaveType);
      await _context.SaveChangesAsync();
      return leaveType;
    }

    public async Task<List<LeaveEntitlementRule>> CreateLeaveEntitlementRules(List<LeaveEntitlementRule> rules)
    {
      await _context.LeaveEntitlementRules.AddRangeAsync(rules);
      await _context.SaveChangesAsync();
      return rules;
    }
    /// <summary>
    /// Retrieves a list of all leave types along with their associated entitlement rules from the database,
    /// maps the data to a list of LeaveTypeResponse DTOs, and returns this list to the caller, 
    /// allowing for the display or further processing of leave type information in the application.
    /// </summary>
    /// <returns></returns>
    public async Task<List<LeaveTypeResponseDto>> GetLeaveTypesAsync()
    {
      var leaveTypes = await _context.LeaveTypes
          .Include(l => l.EntitlementRules)
          .ToListAsync();

      return leaveTypes.Select(s => s.ToLeaveTypeResponseDto()).ToList();
    }

    public async Task<List<EmployeeWithLeaveDto>> GetAllEmployeesWithLeaveAsync()
    {
      return await _context.Employees
              .Include(e => e.Position)
              .Include(e => e.LeaveBalances)
                  .ThenInclude(lb => lb.LeaveType)
              .Select(e => new EmployeeWithLeaveDto
              {
                EmployeeId = e.EmployeeId,
                FullName = e.Name + " " + e.Surname,
                Email = e.Email,
                Position = e.Position!.PositionTitle,
                LeaveBalances = e.LeaveBalances.Select(lb => new LeaveBalanceSummary
                {
                  LeaveType = lb.LeaveType.Name,
                  AccruedDays = lb.AccruedDays,
                  TakenDays = lb.TakenDays,
                  AvailableDays = lb.AvailableDays
                }).ToList()
              })
              .ToListAsync();
    }

    public async Task<EmployeeWithLeaveDto?> GetEmployeeWithLeaveByIdAsync(string employeeId)
    {
      var e = await _context.Employees
          .Include(x => x.Position)
          .Include(x => x.LeaveBalances)
              .ThenInclude(lb => lb.LeaveType)
          .FirstOrDefaultAsync(x => x.EmployeeId == employeeId);

      if (e == null)
        return null;

      return e.ToEmployeeWithLeaveDto();
    }

    public async Task<LeaveTypeResponseDto?> GetLeaveTypeByIdAsync(int id)
    {
      var leaveType = await _context.LeaveTypes
          .Include(l => l.EntitlementRules)
          .FirstOrDefaultAsync(l => l.Id == id);

      if (leaveType == null)
        return null;

      return leaveType.ToLeaveTypeResponseDto();
    }

    public async Task<List<string>?> GetExistingNames(string name)
    {
      var existingNames = await _context.LeaveTypes
          .Select(x => x.Name)
          .ToListAsync();
      if (existingNames == null)
        return null;
      return existingNames;
    }
    public async Task<List<string>?> GetExistingCodes(string code)
    {
      var existingCodes = await _context.LeaveTypes
                .Select(x => x.Code)
                .ToListAsync();
      if (existingCodes == null)
        return null;
      return existingCodes;
    }
  }
}

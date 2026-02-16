namespace HRConnect.Api.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;

    public class EmployeeEntitlementService : IEmployeeEntitlementService
    {
        private readonly ApplicationDBContext _context;

        public EmployeeEntitlementService(ApplicationDBContext context)
        {
            _context = context;
        }

        // ============================================================
        // CREATE EMPLOYEE
        // ============================================================
        public async Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request)
        {
            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = request.PositionId,
                ReportingManagerId = request.ReportingManagerId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Gender = request.Gender,
                StartDate = request.StartDate,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            await InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            return await GetEmployeeByIdAsync(employee.EmployeeId)
                   ?? throw new InvalidOperationException("Failed to load created employee.");
        }

        // ============================================================
        // GET ALL EMPLOYEES
        // ============================================================
        public async Task<List<EmployeeResponse>> GetAllEmployeesAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.Position)
                    .ThenInclude(p => p.JobGrade)
                .Include(e => e.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .ToListAsync();

            return employees.Select(MapToResponse).ToList();
        }

        // ============================================================
        // GET BY ID
        // ============================================================
        public async Task<EmployeeResponse?> GetEmployeeByIdAsync(Guid id)
        {
            var employee = await _context.Employees
                .Include(e => e.Position)
                    .ThenInclude(p => p.JobGrade)
                .Include(e => e.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
                return null;

            return MapToResponse(employee);
        }

        // ============================================================
        // UPDATE POSITION
        // ============================================================
        public async Task<EmployeeResponse> UpdateEmployeePositionAsync(Guid employeeId, int newPositionId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            employee.PositionId = newPositionId;
            employee.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await RecalculateAnnualLeaveAsync(employeeId);

            return await GetEmployeeByIdAsync(employeeId)
                   ?? throw new InvalidOperationException("Failed to load updated employee.");
        }

        // ============================================================
        // DELETE
        // ============================================================
        public async Task DeleteEmployeeAsync(Guid id)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }

        // ============================================================
        // INITIALIZE LEAVE BALANCES
        // ============================================================
        public async Task InitializeEmployeeLeaveBalancesAsync(Guid employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Position)
                    .ThenInclude(p => p.JobGrade)
                .Include(e => e.LeaveBalances)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            var yearsOfService = CalculateYearsOfService(employee.StartDate);
            var leaveTypes = await _context.LeaveTypes
                .Where(l => l.IsActive)
                .ToListAsync();

            var currentYear = DateTime.UtcNow.Year;

            foreach (var leaveType in leaveTypes)
            {
                if (leaveType.FemaleOnly && employee.Gender != "Female")
                    continue;

                if (employee.LeaveBalances.Any(b => b.LeaveTypeId == leaveType.Id))
                    continue;

                var rule = await _context.LeaveEntitlementRules
                    .FirstOrDefaultAsync(r =>
                        r.LeaveTypeId == leaveType.Id &&
                        r.JobGradeId == employee.Position.JobGradeId &&
                        r.MinYearsService <= yearsOfService &&
                        (r.MaxYearsService == null || r.MaxYearsService >= yearsOfService) &&
                        r.IsActive);

                if (rule == null)
                    continue;

                decimal entitledDays = rule.DaysAllocated;

                // Mid-year prorating (Annual only)
                if (leaveType.Code == "AL" &&
                    employee.StartDate.Year == currentYear)
                {
                    int monthsRemaining = 12 - employee.StartDate.Month + 1;
                    entitledDays = Math.Round((rule.DaysAllocated / 12m) * monthsRemaining, 2);
                }

                var balance = new EmployeeLeaveBalance
                {
                    EmployeeId = employee.EmployeeId,
                    LeaveTypeId = leaveType.Id,
                    EntitledDays = entitledDays,
                    UsedDays = 0,
                    AccruedDays = 0,
                    RemainingDays = entitledDays
                };

                await _context.EmployeeLeaveBalances.AddAsync(balance);
            }

            await _context.SaveChangesAsync();
        }

        // ============================================================
        // ANNUAL RECALCULATION
        // ============================================================
        public async Task RecalculateAnnualLeaveAsync(Guid employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Position)
                .ThenInclude(p => p.JobGrade)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            if (!employee.UpdatedDate.HasValue)
                throw new InvalidOperationException("UpdatedDate not set.");

            var changeDate = DateOnly.FromDateTime(employee.UpdatedDate.Value);

            var annualLeave = await _context.LeaveTypes
                .FirstAsync(l => l.Code == "AL" && l.IsActive);

            var balance = await _context.EmployeeLeaveBalances
                .FirstAsync(b =>
                    b.EmployeeId == employeeId &&
                    b.LeaveTypeId == annualLeave.Id);

            var yearsOfService = CalculateYearsOfService(employee.StartDate);

            // 🔹 NEW rule (after promotion)
            var newRule = await _context.LeaveEntitlementRules
                .FirstAsync(r =>
                    r.LeaveTypeId == annualLeave.Id &&
                    r.JobGradeId == employee.Position.JobGradeId &&
                    r.MinYearsService <= yearsOfService &&
                    (r.MaxYearsService == null || r.MaxYearsService >= yearsOfService) &&
                    r.IsActive);

            // 🔹 OLD rule (before promotion)
            var oldRule = await _context.LeaveEntitlementRules
                .FirstAsync(r =>
                    r.LeaveTypeId == annualLeave.Id &&
                    r.JobGradeId != employee.Position.JobGradeId &&
                    r.MinYearsService <= yearsOfService &&
                    (r.MaxYearsService == null || r.MaxYearsService >= yearsOfService) &&
                    r.IsActive);

            int monthsBefore = changeDate.Month - 1;
            int monthsAfter = 12 - monthsBefore;

            decimal before = (oldRule.DaysAllocated / 12m) * monthsBefore;
            decimal after = (newRule.DaysAllocated / 12m) * monthsAfter;

            var total = Math.Round(before + after, 2);

            balance.EntitledDays = total;
            balance.RemainingDays = total - balance.UsedDays;

            await _context.SaveChangesAsync();
        }


        // ============================================================
        // YEARS CALCULATION
        // ============================================================
        private decimal CalculateYearsOfService(DateOnly startDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (startDate > today)
                return 0;

            var totalDays = today.DayNumber - startDate.DayNumber;
            return Math.Round(totalDays / 365.25m, 2);
        }


        // ============================================================
        // MAPPING
        // ============================================================
        private EmployeeResponse MapToResponse(Employee e)
        {
            var annual = e.LeaveBalances
                .FirstOrDefault(lb => lb.LeaveType.Code == "AL");

            return new EmployeeResponse
            {
                Id = e.EmployeeId,
                FullName = $"{e.FirstName} {e.LastName}",
                Gender = e.Gender,
                Position = e.Position.Title,
                JobGrade = e.Position.JobGrade.Name,
                StartDate = e.StartDate,
                AnnualLeaveRemaining = annual?.RemainingDays ?? 0,
                LeaveBalances = e.LeaveBalances.Select(lb => new LeaveBalanceSummary
                {
                    LeaveType = lb.LeaveType.Name,
                    EntitledDays = lb.EntitledDays,
                    UsedDays = lb.UsedDays,
                    RemainingDays = lb.RemainingDays
                }).ToList()
            };
        }
    }
}

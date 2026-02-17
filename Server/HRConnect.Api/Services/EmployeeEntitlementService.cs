namespace HRConnect.Api.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Utils;

    public class EmployeeEntitlementService : IEmployeeEntitlementService
    {
        private readonly ApplicationDBContext _context;
        private readonly IEmailService _emailService;

        public EmployeeEntitlementService(ApplicationDBContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
        // UPDATE POSITION (Promotion/Demotion)
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
        // DELETE (Will be removed)
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

            //NEW rule (after promotion/demotion)
            var newRule = await _context.LeaveEntitlementRules
                .FirstAsync(r =>
                    r.LeaveTypeId == annualLeave.Id &&
                    r.JobGradeId == employee.Position.JobGradeId &&
                    r.MinYearsService <= yearsOfService &&
                    (r.MaxYearsService == null || r.MaxYearsService >= yearsOfService) &&
                    r.IsActive);

            //OLD rule (before promotion/demotion)
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
        //Annual Carryover Logic Methods
        private decimal CalculateCarryover(decimal remaining)
        {
            if (remaining <= 0)
                return 0;

            return remaining <= 5 ? remaining : 5;
        }
        public async Task ProcessCarryOverNotificationAsync()
        {
            var today = DateTime.UtcNow.Date;

            if (today.Month != 12 || today.Day != 1)
                return;

            var annualLeave = await _context.LeaveTypes
            .FirstOrDefaultAsync(l => l.Code == "AL" && l.IsActive);

            if (annualLeave == null)
                throw new InvalidOperationException("Annual Leave not configured");

            var balances = await _context.EmployeeLeaveBalances
            .Include(b => b.Employee)
            .Where(b =>
                b.LeaveTypeId == annualLeave.Id &&
                b.RemainingDays > 5)
            .ToListAsync();

            foreach (var balance in balances)
            {
                var forfeited = balance.RemainingDays - 5;
                var subject = "Annual Leave Carryover Warning";
                var body = $@"
                Dear {balance.Employee.FirstName},
                
                You currently have {balance.RemainingDays} days of Annual Leave remaining.
                
                Only 5 days can be carried over into the next year. 
                {forfeited} days will be forfeited if its not used before 31 December.
                
                Regards,
                HRConnect
                ";

                await _emailService.SendEmailAsync(
                    balance.Employee.ReportingManagerId, //Change this Later
                    subject,
                    body
                );
            }
        }
        public async Task ProcessAnnualResetAsync()
        {
            var today = DateTime.UtcNow.Date;

            if (today.Month != 1 || today.Day != 1)
                return;

            var annualLeave = await _context.LeaveTypes
                .FirstOrDefaultAsync(l => l.Code == "AL" && l.IsActive);

            if (annualLeave == null)
                throw new InvalidOperationException("Annual Leave not configured.");

            var balances = await _context.EmployeeLeaveBalances
                .Include(b => b.Employee)
                    .ThenInclude(e => e.Position)
                    .ThenInclude(p => p.JobGrade)
                .Where(b => b.LeaveTypeId == annualLeave.Id)
                .ToListAsync();

            foreach (var balance in balances)
            {
                var employee = balance.Employee;

                var years = CalculateYearsOfService(employee.StartDate);

                var rule = await _context.LeaveEntitlementRules
                    .FirstAsync(r =>
                        r.LeaveTypeId == annualLeave.Id &&
                        r.JobGradeId == employee.Position.JobGradeId &&
                        r.MinYearsService <= years &&
                        (r.MaxYearsService == null || r.MaxYearsService >= years) &&
                        r.IsActive);

                var carryover = CalculateCarryover(balance.RemainingDays);

                var newEntitlement = rule.DaysAllocated + carryover;

                balance.EntitledDays = newEntitlement;
                balance.UsedDays = 0;
                balance.RemainingDays = newEntitlement;
            }

            await _context.SaveChangesAsync();
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

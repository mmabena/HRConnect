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
            await RecalculateAllSickLeaveAsync();

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
            await RecalculateAllSickLeaveAsync();

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
        public async Task UpdateUsedDaysAsync(UpdateUsedDaysRequest request)
        {
            if (request.UsedDays < 0)
                throw new InvalidOperationException("Used days cannot be negative.");

            var balance = await _context.EmployeeLeaveBalances
                .Include(b => b.LeaveType)
                .Include(b => b.Employee)
                .FirstOrDefaultAsync(b =>
                    b.EmployeeId == request.EmployeeId &&
                    b.LeaveTypeId == request.LeaveTypeId);

            if (balance == null)
                throw new InvalidOperationException("Leave balance not found.");

            // Ensure sick leave is up to date before validation
            if (balance.LeaveType.Code == "SL")
                await RecalculateSickLeaveAsync(request.EmployeeId);

            if (request.UsedDays > balance.EntitledDays)
                throw new InvalidOperationException(
                    "Used days cannot exceed entitled days.");

            balance.UsedDays = request.UsedDays;
            balance.RemainingDays =
                balance.EntitledDays - balance.UsedDays;

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
                // =============================
                // SICK LEAVE — delegate to recalculation method
                // =============================
                if (leaveType.Code == "SL")
                {
                    // Create initial balance with rule default (30)
                    var sickBalance = new EmployeeLeaveBalance
                    {
                        EmployeeId = employee.EmployeeId,
                        LeaveTypeId = leaveType.Id,
                        EntitledDays = rule.DaysAllocated, // temporary
                        UsedDays = 0,
                        AccruedDays = 0,
                        RemainingDays = rule.DaysAllocated
                    };

                    await _context.EmployeeLeaveBalances.AddAsync(sickBalance);
                    await _context.SaveChangesAsync();

                    await RecalculateSickLeaveAsync(employee.EmployeeId);

                    continue; // skip normal balance creation
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

            // NEW rule (after promotion/demotion)
            var newRule = await _context.LeaveEntitlementRules
                .FirstAsync(r =>
                    r.LeaveTypeId == annualLeave.Id &&
                    r.JobGradeId == employee.Position.JobGradeId &&
                    r.MinYearsService <= yearsOfService &&
                    (r.MaxYearsService == null || r.MaxYearsService >= yearsOfService) &&
                    r.IsActive);

            // OLD rule (before promotion/demotion)
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

            // ===============================
            // EMAIL NOTIFICATION SECTION
            // ===============================

            await _emailService.SendEmailAsync(
                employee.Email, // Make sure Employee model has Email property
                "Annual Leave Recalculated Due to Position Change",
                $"""
        Dear {employee.FirstName},

        Your position has recently been updated to: {employee.Position.Title}.

        As a result, your annual leave entitlement has been recalculated.

        New Annual Entitlement: {balance.EntitledDays} days
        Used Days: {balance.UsedDays} days
        Remaining Days: {balance.RemainingDays} days

        This adjustment was calculated proportionally based on the month of change.

        If you have any questions, please contact HR.

        Regards,
        HRConnect
        """
            );
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
            {
#if !DEBUG
                return;
#endif
            }


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
                    balance.Employee.Email, //Change this Later
                    subject,
                    body
                );
            }
        }
        public async Task ProcessAnnualResetAsync()
        {
            var today = DateTime.UtcNow.Date;
            var currentYear = today.Year;

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
                // IDEMPOTENCY CHECK
                if (balance.LastResetYear == currentYear)
                    continue;

                var employee = balance.Employee;
                var years = CalculateYearsOfService(employee.StartDate);

                var rule = await _context.LeaveEntitlementRules
                    .FirstAsync(r =>
                        r.LeaveTypeId == annualLeave.Id &&
                        r.JobGradeId == employee.Position.JobGradeId &&
                        r.MinYearsService <= years &&
                        (r.MaxYearsService == null || r.MaxYearsService >= years) &&
                        r.IsActive);

                var remainingBeforeReset = balance.RemainingDays;

                var carryover = CalculateCarryover(remainingBeforeReset);
                var forfeited = remainingBeforeReset > 5
                    ? remainingBeforeReset - 5
                    : 0;

                var newEntitlement = rule.DaysAllocated + carryover;

                // STORE AUDIT VALUES
                balance.CarryoverDays = carryover;
                balance.ForfeitedDays = forfeited;

                balance.EntitledDays = newEntitlement;
                balance.UsedDays = 0;
                balance.RemainingDays = newEntitlement;

                balance.LastResetYear = currentYear;
            }

            await _context.SaveChangesAsync();
        }
        public async Task UpdateLeaveEntitlementRuleAsync(UpdateLeaveRuleRequest request)
        {
            if (request.NewDaysAllocated < 0)
                throw new InvalidOperationException("Days allocated cannot be negative.");

            var rule = await _context.LeaveEntitlementRules
                .Include(r => r.LeaveType)
                .FirstOrDefaultAsync(r => r.Id == request.RuleId);

            if (rule == null)
                throw new InvalidOperationException("Rule not found.");

            if (rule.MinYearsService < 0)
                throw new InvalidOperationException("MinYearsService cannot be negative.");

            if (rule.MaxYearsService.HasValue &&
                rule.MaxYearsService < rule.MinYearsService)
                throw new InvalidOperationException("MaxYearsService cannot be less than MinYearsService.");

            // Get affected employees BEFORE updating rule
            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.LeaveBalances)
                .Where(e => e.Position.JobGradeId == rule.JobGradeId)
                .ToListAsync();

            foreach (var employee in employees)
            {
                var years = CalculateYearsOfService(employee.StartDate);
                // If the employee's years of service is LESS than the minimum
                // required by this entitlement rule, skip this rule and move
                if (years < rule.MinYearsService)
                    continue;
                // If the rule has a maximum years-of-service limit defined
                // AND the employee's years exceed that maximum, skip this employee
                if (rule.MaxYearsService.HasValue &&
                    years > rule.MaxYearsService.Value)
                    continue;
                // Retrieve the employee's leave balance record
                // that matches the current rule's LeaveTypeId.
                var balance = employee.LeaveBalances
                    .FirstOrDefault(lb => lb.LeaveTypeId == rule.LeaveTypeId);

                if (balance == null)
                    continue;

                // PROTECTION: cannot reduce below used days
                if (request.NewDaysAllocated < balance.UsedDays)
                    throw new InvalidOperationException(
                        $"Cannot reduce entitlement below used days for employee {employee.FirstName}.");
            }

            // Safe to update rule
            rule.DaysAllocated = request.NewDaysAllocated;

            await _context.SaveChangesAsync();

            // Recalculate employees
            await RecalculateEmployeesForRuleChangeAsync(rule);
        }
        public async Task RecalculateEmployeesForRuleChangeAsync(LeaveEntitlementRule rule)
        {
            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .Where(e => e.Position.JobGradeId == rule.JobGradeId)
                .ToListAsync();

            foreach (var employee in employees)
            {
                var years = CalculateYearsOfService(employee.StartDate);

                if (years < rule.MinYearsService)
                    continue;

                if (rule.MaxYearsService.HasValue &&
                    years > rule.MaxYearsService.Value)
                    continue;

                var balance = employee.LeaveBalances
                    .FirstOrDefault(lb => lb.LeaveTypeId == rule.LeaveTypeId);

                if (balance == null)
                    continue;

                balance.EntitledDays = rule.DaysAllocated;
                balance.RemainingDays = rule.DaysAllocated - balance.UsedDays;

                await _emailService.SendEmailAsync(
                    employee.Email,
                    "Leave Policy Updated",
                    $"""
            Dear {employee.FirstName},

            The company has updated the leave policy.

            Your new annual entitlement is {rule.DaysAllocated} days.
            Your remaining balance is now {balance.RemainingDays} days.

            Regards,
            HRConnect
            """);
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
        public async Task RecalculateSickLeaveAsync(Guid employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.LeaveBalances)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            var sickLeave = await _context.LeaveTypes
                .FirstOrDefaultAsync(l => l.Code == "SL" && l.IsActive);

            if (sickLeave == null)
                throw new InvalidOperationException("Sick Leave not configured.");

            var balance = employee.LeaveBalances
                .FirstOrDefault(b => b.LeaveTypeId == sickLeave.Id);

            if (balance == null)
                return;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var monthsWorked =
                (today.Year - employee.StartDate.Year) * 12 +
                (today.Month - employee.StartDate.Month);

            if (monthsWorked < 0)
                monthsWorked = 0;

            decimal entitledDays;

            // -------------------------
            // PHASE 1 — First 6 Months
            // -------------------------
            if (monthsWorked < 6)
            {
                entitledDays = monthsWorked;
            }
            else
            {
                entitledDays = 30;
            }

            // -------------------------
            // PHASE 3 — 36-Month Reset
            // -------------------------
            if (monthsWorked >= 36)
            {
                balance.UsedDays = 0;
                entitledDays = 30;
            }

            balance.EntitledDays = entitledDays;
            balance.RemainingDays = Math.Max(0, entitledDays - balance.UsedDays);

            await _context.SaveChangesAsync();
        }
        public async Task RecalculateAllSickLeaveAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            foreach (var employee in employees)
            {
                var sickBalance = employee.LeaveBalances
                    .FirstOrDefault(b => b.LeaveType.Code == "SL");

                if (sickBalance == null)
                    continue;

                var monthsWorked =
                    (today.Year - employee.StartDate.Year) * 12 +
                    (today.Month - employee.StartDate.Month);

                if (monthsWorked < 0)
                    monthsWorked = 0;

                decimal entitled =
                    monthsWorked < 6 ? monthsWorked : 30;

                // 36-month automatic reset
                if (monthsWorked >= 36)
                {
                    sickBalance.UsedDays = 0;
                    entitled = 30;
                }

                sickBalance.EntitledDays = entitled;
                sickBalance.RemainingDays =
                    Math.Max(0, entitled - sickBalance.UsedDays);
            }

            await _context.SaveChangesAsync();
        }
    }
}

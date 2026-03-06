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
        public async Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request)
        {
            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                PositionId = request.PositionId,
                ReportingManagerId = request.ReportingManagerId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Gender = request.Gender,
                StartDate = request.StartDate,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // initialization now takes responsibility for creating the first accrual segment
            await InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            return await GetEmployeeByIdAsync(employee.EmployeeId)
                   ?? throw new InvalidOperationException("Failed to load created employee.");
        }

        public async Task<List<EmployeeResponse>> GetAllEmployeesAsync()
        {
            // Recalculate Annual for ALL employees first
            var employeeIds = await _context.Employees
                .Select(e => e.EmployeeId)
                .ToListAsync();

            foreach (var id in employeeIds)
            {
                await RecalculateAnnualLeaveAsync(id);
            }

            // Existing recalculations remain untouched
            await RecalculateAllSickLeaveAsync();
            await RecalculateAllFamilyResponsibilityLeaveAsync();

            var employees = await _context.Employees
                .Include(e => e.Position)
                    .ThenInclude(p => p.JobGrade)
                .Include(e => e.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .ToListAsync();

            return employees.Select(MapToResponse).ToList();
        }
        public async Task<EmployeeResponse?> GetEmployeeByIdAsync(Guid id)
        {
            await RecalculateAnnualLeaveAsync(id);
            await RecalculateAllSickLeaveAsync();
            await RecalculateAllFamilyResponsibilityLeaveAsync();

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
        public async Task<EmployeeResponse> UpdateEmployeePositionAsync(Guid employeeId, int newPositionId)
        {
            var employee = await _context.Employees
                .Include(e => e.Position)
                    .ThenInclude(p => p.JobGrade)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            // If position not actually changing → do nothing
            if (employee.PositionId == newPositionId)
                return await GetEmployeeByIdAsync(employeeId)
                       ?? throw new InvalidOperationException("Failed to load employee.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // ============================================================
            // 1️⃣ Close or overwrite current active accrual segment
            // ============================================================

            var currentSegment = await _context.EmployeeAccrualRateHistories
                .Where(x => x.EmployeeId == employeeId && x.EffectiveTo == null)
                .FirstOrDefaultAsync();

            if (currentSegment != null)
            {
                // 🔥 SAME-DAY CHANGE RULE:
                // If the active segment already started today,
                // remove it entirely (overwrite previous change today)
                if (currentSegment.EffectiveFrom == today)
                {
                    _context.EmployeeAccrualRateHistories.Remove(currentSegment);
                }
                else
                {
                    // Otherwise close it cleanly today
                    currentSegment.EffectiveTo = today.AddDays(-1); // Effective until end of yesterday
                }
            }

            // ============================================================
            // 2️⃣ Update employee position
            // ============================================================

            employee.PositionId = newPositionId;
            employee.UpdatedDate = DateTime.UtcNow;
            await _context.Entry(employee)
                .Reference(e => e.Position)
                .LoadAsync();

            await _context.Entry(employee.Position)
                .Reference(p => p.JobGrade)
                .LoadAsync();

            // ============================================================
            // 3️⃣ Determine new entitlement rule
            // ============================================================

            var annualLeave = await _context.LeaveTypes
                .FirstAsync(l => l.Code == "AL" && l.IsActive);

            var yearsOfService = CalculateYearsOfService(employee.StartDate);

            var newRule = await _context.LeaveEntitlementRules
                .FirstAsync(r =>
                    r.LeaveTypeId == annualLeave.Id &&
                    r.JobGradeId == employee.Position.JobGradeId &&
                    r.MinYearsService <= yearsOfService &&
                    (r.MaxYearsService == null || r.MaxYearsService >= yearsOfService) &&
                    r.IsActive);

            // ============================================================
            // 4️⃣ Create new accrual segment STARTING TODAY
            // ============================================================

            await _context.EmployeeAccrualRateHistories.AddAsync(
                new EmployeeAccrualRateHistory
                {
                    EmployeeId = employeeId,
                    AnnualEntitlement = newRule.DaysAllocated,
                    DailyRate = (newRule.DaysAllocated / 12m) / 21.67m,
                    EffectiveFrom = today,   // starts immediately
                    EffectiveTo = null,
                    CreatedDate = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            // ============================================================
            // 5️⃣ Recalculate using full historical model
            // ============================================================

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

            if (request.UsedDays > balance.AvailableDays)
                throw new InvalidOperationException(
                    "Used days cannot exceed available days.");

            balance.UsedDays = request.UsedDays;
            if (balance.LeaveType.Code == "AL")
            {
                balance.AvailableDays =
                    balance.CarryoverDays +
                    balance.EntitledDays -
                    balance.UsedDays;
            }
            else
            {
                balance.AvailableDays =
                    balance.EntitledDays - balance.UsedDays;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException(
                    "This leave balance was modified by another process. Please refresh and try again.");
            }
        }
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

                // =============================
                // ANNUAL LEAVE — Dynamic Accrual
                // =============================
                if (leaveType.Code == "AL")
                {
                    var annualBalance = new EmployeeLeaveBalance
                    {
                        EmployeeId = employee.EmployeeId,
                        LeaveTypeId = leaveType.Id,
                        EntitledDays = 0,
                        UsedDays = 0,
                        AccruedDays = 0,
                        AvailableDays = 0,
                        CarryoverDays = 0,
                        ForfeitedDays = 0,
                        LastResetYear = DateTime.UtcNow.Year
                    };

                    await _context.EmployeeLeaveBalances.AddAsync(annualBalance);
                    await _context.SaveChangesAsync();
                    await BackfillHistoricalAnnualAccrualAsync(employee);

                    // make sure there's an accrual rate segment - if one already exists
                    // (e.g. when the employee record was just created) this call is
                    // a no-op because CreateInitialAccrualSegmentAsync checks the
                    // context before adding.
                    var hasSegment = await _context.EmployeeAccrualRateHistories
                        .AnyAsync(s => s.EmployeeId == employee.EmployeeId);
                    if (!hasSegment)
                    {
                        await CreateInitialAccrualSegmentAsync(employee);
                    }

                    continue;
                }

                // =============================
                // SICK LEAVE
                // =============================
                if (leaveType.Code == "SL")
                {
                    var sickBalance = new EmployeeLeaveBalance
                    {
                        EmployeeId = employee.EmployeeId,
                        LeaveTypeId = leaveType.Id,
                        EntitledDays = rule.DaysAllocated,
                        UsedDays = 0,
                        AccruedDays = 0,
                        AvailableDays = rule.DaysAllocated
                    };

                    await _context.EmployeeLeaveBalances.AddAsync(sickBalance);
                    await _context.SaveChangesAsync();

                    await RecalculateSickLeaveAsync(employee.EmployeeId);
                    continue;
                }

                // =============================
                // FAMILY RESPONSIBILITY LEAVE
                // =============================
                if (leaveType.Code == "FRL")
                {
                    var frlBalance = new EmployeeLeaveBalance
                    {
                        EmployeeId = employee.EmployeeId,
                        LeaveTypeId = leaveType.Id,
                        EntitledDays = rule.DaysAllocated,
                        UsedDays = 0,
                        AccruedDays = 0,
                        AvailableDays = rule.DaysAllocated,
                        LastResetYear = DateTime.UtcNow.Year
                    };

                    await _context.EmployeeLeaveBalances.AddAsync(frlBalance);
                    await _context.SaveChangesAsync();

                    await RecalculateFamilyResponsibilityLeaveAsync(employee.EmployeeId);
                    continue;
                }

                // =============================
                // OTHER LEAVE TYPES
                // =============================
                var balance = new EmployeeLeaveBalance
                {
                    EmployeeId = employee.EmployeeId,
                    LeaveTypeId = leaveType.Id,
                    EntitledDays = rule.DaysAllocated,
                    UsedDays = 0,
                    AccruedDays = 0,
                    AvailableDays = rule.DaysAllocated
                };

                await _context.EmployeeLeaveBalances.AddAsync(balance);
            }

            await _context.SaveChangesAsync();
        }
        public async Task RecalculateAnnualLeaveAsync(Guid employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.LeaveBalances)
                .Include(e => e.Position)
                    .ThenInclude(p => p.JobGrade)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            var annualLeave = await _context.LeaveTypes
                .FirstAsync(l => l.Code == "AL" && l.IsActive);

            var balance = employee.LeaveBalances
                .First(b => b.LeaveTypeId == annualLeave.Id);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Determine cycle start (1 Jan or employee start if hired this year)
            var cycleStart = employee.StartDate.Year == today.Year
                ? employee.StartDate
                : new DateOnly(today.Year, 1, 1);

            // 🔥 IMPORTANT: Use ALL segments, not just active one
            var segments = await _context.EmployeeAccrualRateHistories
                .Where(x => x.EmployeeId == employeeId)
                .OrderBy(x => x.EffectiveFrom)
                .ToListAsync();

            if (segments.Count == 0)
                return;

            decimal totalAccrued = 0m;

            foreach (var segment in segments)
            {
                // Determine effective window inside current cycle
                var segmentStart = segment.EffectiveFrom > cycleStart
                    ? segment.EffectiveFrom
                    : cycleStart;

                var segmentEnd = segment.EffectiveTo.HasValue && segment.EffectiveTo.Value < today
                    ? segment.EffectiveTo.Value
                    : today;

                if (segmentEnd < segmentStart)
                    continue;

                int workingDays = WorkingDayCalculator.CountWorkingDays(
                    segmentStart,
                    segmentEnd);

                totalAccrued += workingDays * segment.DailyRate;
            }

            totalAccrued = Math.Round(totalAccrued, 2);

            balance.EntitledDays = totalAccrued;

            // Available = Carryover + AccruedThisYear − Used
            balance.AvailableDays =
                balance.CarryoverDays +
                totalAccrued -
                balance.UsedDays;
            balance.LastCalculatedDate = today;

            await _context.SaveChangesAsync();

            // ===============================
            // EMAIL NOTIFICATION SECTION
            // ===============================
            try
            {
                await _emailService.SendEmailAsync(
               employee.Email,
               "Annual Leave Recalculated Due to Position Change",
               $"""
Dear {employee.FirstName},

Your position has recently been updated to: {employee.Position.Title}.

As a result, your annual leave entitlement has been recalculated.

New Annual Entitlement: {balance.EntitledDays} days
Used Days: {balance.UsedDays} days
Available Days: {balance.AvailableDays} days

This adjustment was calculated proportionally based on the promotion date.

If you have any questions, please contact HR.

Regards,
HRConnect
"""
           );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
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
                b.AvailableDays > 5)
            .ToListAsync();

            foreach (var balance in balances)
            {
                var forfeited = balance.AvailableDays - 5;
                var subject = "Annual Leave Carryover Warning";
                var body = $@"
                Dear {balance.Employee.FirstName},
                
                You currently have {balance.AvailableDays} days of Annual Leave remaining.
                
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
        public async Task ProcessAnnualResetAsync(int? overrideYear = null)
        {

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var today = DateTime.UtcNow.Date;

                // If overrideYear provided → use it
                // Otherwise use real current year
                var currentYear = overrideYear ?? today.Year;

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
                    // ================================
                    // 🔹 STORE CLOSED YEAR SNAPSHOT (LEDGER CORRECT)
                    // ================================

                    var yearToClose = currentYear - 1;

                    // Snapshot values BEFORE reset
                    var openingBalance = balance.CarryoverDays;
                    var accrued = balance.EntitledDays;
                    var used = balance.UsedDays;

                    // True closing balance before forfeiture
                    var closingBalance = openingBalance + accrued - used;

                    // Carryover applied to next year
                    var carryoverApplied = CalculateCarryover(closingBalance);

                    // True forfeiture amount
                    var forfeited = closingBalance - carryoverApplied;

                    var alreadyExists = await _context.AnnualLeaveAccrualHistories
                        .AnyAsync(x =>
                            x.EmployeeId == balance.EmployeeId &&
                            x.Year == yearToClose);

                    if (!alreadyExists)
                    {
                        await _context.AnnualLeaveAccrualHistories.AddAsync(
                            new AnnualLeaveAccrualHistory
                            {
                                EmployeeId = balance.EmployeeId,
                                Year = yearToClose,

                                OpeningBalance = openingBalance,
                                Accrued = accrued,
                                Used = used,
                                Forfeited = forfeited,
                                ClosingBalance = closingBalance,

                                CreatedDate = DateTime.UtcNow
                            });
                    }

                    // ================================
                    // 🔹 RESET LIVE STATE (UNCHANGED LOGIC)
                    // ================================

                    balance.CarryoverDays = carryoverApplied;
                    balance.ForfeitedDays = 0;

                    balance.EntitledDays = 0;
                    balance.AccruedDays = 0;
                    balance.AvailableDays = carryoverApplied;

                    balance.UsedDays = 0;
                    balance.LastResetYear = currentYear;
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error during annual reset: {ex.Message}");
            }


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

            var employeeIds = employees.Select(e => e.EmployeeId).ToList();

            // Load all active accrual segments in one query
            var segments = await _context.EmployeeAccrualRateHistories
                .Where(x => employeeIds.Contains(x.EmployeeId) && x.EffectiveTo == null)
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

                // Find segment in memory instead of querying DB
                var segment = segments
                    .FirstOrDefault(x => x.EmployeeId == employee.EmployeeId);

                if (segment != null)
                {
                    segment.AnnualEntitlement = rule.DaysAllocated;
                    segment.DailyRate = rule.DaysAllocated / 260m;
                }

                // Recalculate using existing engine
                await RecalculateAnnualLeaveAsync(employee.EmployeeId);

                var updatedBalance = employee.LeaveBalances
                    .First(lb => lb.LeaveTypeId == rule.LeaveTypeId);

                await _emailService.SendEmailAsync(
                    employee.Email,
                    "Leave Policy Updated",
                    $"""
Dear {employee.FirstName},

The company has updated the leave policy.

Your new annual entitlement is {rule.DaysAllocated} days.
Your available balance is now {updatedBalance.AvailableDays} days.

Regards,
HRConnect
""");
            }

            await _context.SaveChangesAsync();
        }
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
                AnnualLeaveRemaining = annual?.AvailableDays ?? 0,
                LeaveBalances = e.LeaveBalances.Select(lb => new LeaveBalanceSummary
                {
                    LeaveType = lb.LeaveType.Name,
                    EntitledDays = lb.EntitledDays,
                    UsedDays = lb.UsedDays,
                    AvailableDays = lb.AvailableDays
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
                (today.Month - employee.StartDate.Month) + 1;

            if (monthsWorked < 0)
                monthsWorked = 0;

            decimal entitledDays;

            // =========================
            // Phase 1 – First 6 Months
            // =========================
            if (monthsWorked < 6)
            {
                entitledDays = monthsWorked;
            }
            else
            {
                entitledDays = 30;
            }

            // =========================
            // 36-Month Cycle Reset
            // =========================

            var cycleNumber = monthsWorked / 36;

            if (balance.LastResetYear == null || balance.LastResetYear != cycleNumber)
            {
                balance.UsedDays = 0;
                balance.LastResetYear = cycleNumber;
            }

            balance.EntitledDays = entitledDays;
            balance.AvailableDays = Math.Max(0, entitledDays - balance.UsedDays);

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
                    (today.Month - employee.StartDate.Month) + 1;

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
                sickBalance.AvailableDays =
                    Math.Max(0, entitled - sickBalance.UsedDays);
            }

            await _context.SaveChangesAsync();
        }
        public async Task RecalculateFamilyResponsibilityLeaveAsync(Guid employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.LeaveBalances)
                .ThenInclude(lb => lb.LeaveType)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            var frlBalance = employee.LeaveBalances
                .FirstOrDefault(b => b.LeaveType.Code == "FRL");

            if (frlBalance == null)
                return;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var month = employee.StartDate.Month;

            var day = Math.Min(
                employee.StartDate.Day,
                DateTime.DaysInMonth(today.Year, month)
            );

            var anniversaryThisYear = new DateOnly(
                today.Year,
                month,
                day
            );

            // If anniversary hasn’t happened yet this year,
            // use last year as current cycle
            if (today < anniversaryThisYear)
                anniversaryThisYear = anniversaryThisYear.AddYears(-1);

            var anniversaryYear = anniversaryThisYear.Year;

            // Reset only if this cycle hasn't already been processed
            if (frlBalance.LastResetYear == null ||
                frlBalance.LastResetYear != anniversaryYear)
            {
                frlBalance.UsedDays = 0;
                frlBalance.EntitledDays = 3;
                frlBalance.AvailableDays = 3;
                frlBalance.LastResetYear = anniversaryYear;

                await _context.SaveChangesAsync();
            }
        }
        //Batch Calculation
        public async Task RecalculateAllFamilyResponsibilityLeaveAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .ToListAsync();

            foreach (var employee in employees)
            {
                await RecalculateFamilyResponsibilityLeaveAsync(employee.EmployeeId);
            }
        }
        public async Task ResetMaternityLeaveForNewPregnancy(Guid employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            if (employee.Gender != "Female")
                throw new InvalidOperationException("Maternity leave applies to female employees only.");

            var mlBalance = employee.LeaveBalances
                .FirstOrDefault(b => b.LeaveType.Code == "ML");

            if (mlBalance == null)
                throw new InvalidOperationException("Maternity Leave not configured.");

            mlBalance.UsedDays = 0;
            mlBalance.EntitledDays = 120;
            mlBalance.AvailableDays = 120;

            await _context.SaveChangesAsync();
        }
        public async Task<LeaveProjectionResponse> ProjectAnnualLeaveAsync(
     Guid employeeId,
     DateOnly projectionDate)
        {
            var employee = await _context.Employees
                .Include(e => e.LeaveBalances)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            var annualLeave = await _context.LeaveTypes
                .FirstAsync(l => l.Code == "AL" && l.IsActive);

            var balance = employee.LeaveBalances
                .First(b => b.LeaveTypeId == annualLeave.Id);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // ============================================================
            // Cycle start logic
            // ============================================================

            var yearStart = new DateOnly(projectionDate.Year, 1, 1);

            var cycleStart = employee.StartDate > yearStart
                ? employee.StartDate
                : yearStart;

            int totalDaysWorked = 0;

            if (projectionDate >= cycleStart)
            {
                totalDaysWorked = WorkingDayCalculator.CountWorkingDays(
                    cycleStart,
                    projectionDate);
            }

            // ============================================================
            // If projecting today or backwards
            // ============================================================

            if (projectionDate <= today)
            {
                return new LeaveProjectionResponse
                {
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    ProjectionDate = projectionDate,
                    ProjectedEntitledDays = balance.EntitledDays,
                    UsedDays = balance.UsedDays,
                    ProjectedAvailableDays = balance.AvailableDays,
                    DaysWorked = totalDaysWorked
                };
            }

            // ============================================================
            // Load entitlement rules once (avoid DB queries in loop)
            // ============================================================

            var rules = await _context.LeaveEntitlementRules
                .Where(r =>
                    r.LeaveTypeId == annualLeave.Id &&
                    r.JobGradeId == employee.Position.JobGradeId &&
                    r.IsActive)
                .OrderBy(r => r.MinYearsService)
                .ToListAsync();

            // ============================================================
            // Projection engine
            // ============================================================

            decimal projectedAvailable = balance.AvailableDays;
            decimal projectedEntitled = balance.EntitledDays;
            decimal projectedCarryover = balance.CarryoverDays;

            var currentDate = today;

            while (currentDate <= projectionDate)
            {
                var yearEnd = new DateOnly(currentDate.Year, 12, 31);

                var periodStart = currentDate;
                var periodEnd = projectionDate < yearEnd ? projectionDate : yearEnd;

                // ============================================
                // Determine years of service
                // ============================================

                decimal yearsOfService =
                    (periodStart.DayNumber - employee.StartDate.DayNumber) / 365.25m;

                var rule = rules.First(r =>
                    r.MinYearsService <= yearsOfService &&
                    (r.MaxYearsService == null || r.MaxYearsService >= yearsOfService));

                // ============================================
                // Calculate accrual
                // ============================================

                int workingDays = WorkingDayCalculator.CountWorkingDays(
                    periodStart,
                    periodEnd);

                decimal dailyRate =
                    Math.Round((rule.DaysAllocated / 12m) / 21.67m, 6);

                decimal accrued = workingDays * dailyRate;

                projectedEntitled += accrued;

                // Cap entitlement
                if (projectedEntitled > rule.DaysAllocated)
                    projectedEntitled = rule.DaysAllocated;

                projectedAvailable =
                    projectedCarryover +
                    projectedEntitled -
                    balance.UsedDays;

                // ============================================
                // Move to next leave year
                // ============================================

                if (periodEnd == yearEnd && projectionDate > yearEnd)
                {
                    var remaining = projectedAvailable;

                    projectedCarryover = remaining > 5 ? 5 : remaining;

                    projectedEntitled = 0;
                    projectedAvailable = projectedCarryover;

                    currentDate = yearEnd.AddDays(1);
                }
                else
                {
                    break;
                }
            }

            projectedAvailable = Math.Round(projectedAvailable, 2);

            return new LeaveProjectionResponse
            {
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                ProjectionDate = projectionDate,
                ProjectedEntitledDays = projectedEntitled,
                UsedDays = balance.UsedDays,
                ProjectedAvailableDays = projectedAvailable,
                DaysWorked = totalDaysWorked
            };
        }
        private async Task BackfillHistoricalAnnualAccrualAsync(Employee employee)
        {
            var today = DateTime.UtcNow.Date;
            var currentYear = today.Year;

            // If hired this year → nothing to backfill
            if (employee.StartDate.Year >= currentYear)
                return;

            var annualLeave = await _context.LeaveTypes
                .FirstAsync(l => l.Code == "AL" && l.IsActive);

            var balance = await _context.EmployeeLeaveBalances
                .FirstAsync(b =>
                    b.EmployeeId == employee.EmployeeId &&
                    b.LeaveTypeId == annualLeave.Id);

            var rule = await _context.LeaveEntitlementRules
                .FirstAsync(r =>
                    r.LeaveTypeId == annualLeave.Id &&
                    r.JobGradeId == employee.Position.JobGradeId &&
                    r.IsActive);

            decimal dailyRate = (rule.DaysAllocated / 12m) / 21.67m;

            var endOfPreviousYear = new DateOnly(currentYear - 1, 12, 31);

            int workingDays = WorkingDayCalculator.CountWorkingDays(
                employee.StartDate,
                endOfPreviousYear);

            decimal accrued = Math.Round(workingDays * dailyRate, 2);

            var carryover = accrued <= 5 ? accrued : 5;
            var forfeited = accrued > 5 ? accrued - 5 : 0;

            var yearToClose = currentYear - 1;

            // 🔹 Store historical snapshot
            var alreadyExists = await _context.AnnualLeaveAccrualHistories
                .AnyAsync(x =>
                    x.EmployeeId == employee.EmployeeId &&
                    x.Year == yearToClose);

            if (!alreadyExists)
            {
                await _context.AnnualLeaveAccrualHistories.AddAsync(
                    new AnnualLeaveAccrualHistory
                    {
                        EmployeeId = employee.EmployeeId,
                        Year = yearToClose,

                        OpeningBalance = 0,
                        Accrued = accrued,
                        Used = 0,
                        Forfeited = forfeited,
                        ClosingBalance = accrued,

                        CreatedDate = DateTime.UtcNow
                    });
            }

            // 🔹 Set live balance state (NO forfeited stored live)
            balance.CarryoverDays = carryover;
            balance.UsedDays = 0;
            balance.LastResetYear = currentYear;
        }
        private async Task CreateInitialAccrualSegmentAsync(Employee employee)
        {
            // idempotency: don't create a second segment if one already exists
            bool exists = await _context.EmployeeAccrualRateHistories
                .AnyAsync(s => s.EmployeeId == employee.EmployeeId);
            if (exists)
                return;

            var annualLeave = await _context.LeaveTypes
                .FirstAsync(l => l.Code == "AL" && l.IsActive);

            var yearsOfService = CalculateYearsOfService(employee.StartDate);

            var rule = await _context.LeaveEntitlementRules
                .FirstAsync(r =>
                    r.LeaveTypeId == annualLeave.Id &&
                    r.JobGradeId == employee.Position.JobGradeId &&
                    r.MinYearsService <= yearsOfService &&
                    (r.MaxYearsService == null || r.MaxYearsService >= yearsOfService) &&
                    r.IsActive);

            await _context.EmployeeAccrualRateHistories.AddAsync(
                new EmployeeAccrualRateHistory
                {
                    EmployeeId = employee.EmployeeId,
                    AnnualEntitlement = rule.DaysAllocated,
                    DailyRate = (rule.DaysAllocated / 12m) / 21.67m,
                    EffectiveFrom = employee.StartDate,
                    EffectiveTo = null,
                    CreatedDate = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();
        }
    }
}

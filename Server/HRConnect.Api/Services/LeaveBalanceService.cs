namespace HRConnect.Api.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Utils;

    public class LeaveBalanceService : ILeaveBalanceService
    {
        private readonly ApplicationDBContext _context;

        public LeaveBalanceService(ApplicationDBContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Initializes leave balances for a new employee based on their job grade, years of service, and applicable leave rules. 
        /// This should be called when a new employee is created to set up their initial leave entitlements. 
        /// The method checks for each active leave type and applies the relevant entitlement rules to determine the starting balance for each leave type. 
        /// For annual leave, it also backfills historical accruals based on the employee's start date and creates an initial accrual segment if none exist.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task InitializeEmployeeLeaveBalancesAsync(string employeeId)
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
                if (leaveType.FemaleOnly && employee.Gender != Gender.Female)
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

                if (leaveType.Code == "AL")
                {
                    var annualBalance = new EmployeeLeaveBalance
                    {
                        EmployeeId = employee.EmployeeId,
                        LeaveTypeId = leaveType.Id,
                        AccruedDays = 0,
                        TakenDays = 0,
                        AvailableDays = 0,
                        CarryoverDays = 0,
                        ForfeitedDays = 0,
                        LastResetYear = DateTime.UtcNow.Year
                    };

                    await _context.EmployeeLeaveBalances.AddAsync(annualBalance);
                    await _context.SaveChangesAsync();
                    await BackfillHistoricalAnnualAccrualAsync(employee);

                    var hasSegment = await _context.EmployeeAccrualRateHistories
                        .AnyAsync(s => s.EmployeeId == employee.EmployeeId);

                    if (!hasSegment)
                    {
                        await CreateInitialAccrualSegmentAsync(employee);
                    }

                    continue;
                }

                if (leaveType.Code == "SL")
                {
                    var sickBalance = new EmployeeLeaveBalance
                    {
                        EmployeeId = employee.EmployeeId,
                        LeaveTypeId = leaveType.Id,
                        AccruedDays = rule.DaysAllocated,
                        TakenDays = 0,
                        AvailableDays = rule.DaysAllocated
                    };

                    await _context.EmployeeLeaveBalances.AddAsync(sickBalance);
                    await _context.SaveChangesAsync();

                    await RecalculateSickLeaveAsync(employee.EmployeeId);
                    continue;
                }

                if (leaveType.Code == "FRL")
                {
                    var frlBalance = new EmployeeLeaveBalance
                    {
                        EmployeeId = employee.EmployeeId,
                        LeaveTypeId = leaveType.Id,
                        AccruedDays = rule.DaysAllocated,
                        TakenDays = 0,
                        AvailableDays = rule.DaysAllocated,
                        LastResetYear = DateTime.UtcNow.Year
                    };

                    await _context.EmployeeLeaveBalances.AddAsync(frlBalance);
                    await _context.SaveChangesAsync();

                    await RecalculateFamilyResponsibilityLeaveAsync(employee.EmployeeId);
                    continue;
                }

                var balance = new EmployeeLeaveBalance
                {
                    EmployeeId = employee.EmployeeId,
                    LeaveTypeId = leaveType.Id,
                    AccruedDays = rule.DaysAllocated,
                    TakenDays = 0,
                    AvailableDays = rule.DaysAllocated
                };

                await _context.EmployeeLeaveBalances.AddAsync(balance);
            }

            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Updates the taken days for a specific leave type and employee. 
        /// This method validates that the taken days do not exceed the available days and recalculates the available balance accordingly. 
        /// If the leave type is sick leave, it also triggers a recalculation of the sick leave balance to ensure it remains accurate based on the employee's tenure. 
        /// The method handles concurrency issues by catching DbUpdateConcurrencyException,
        /// and throwing a user-friendly error message if the leave balance was modified by another process during the update.   
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task UpdateTakenDaysAsync(UpdateTakenDaysRequest request)
        {
            if (request.TakenDays < 0)
                throw new InvalidOperationException("Used days cannot be negative.");

            var balance = await _context.EmployeeLeaveBalances
                .Include(b => b.LeaveType)
                .Include(b => b.Employee)
                .FirstOrDefaultAsync(b =>
                    b.EmployeeId == request.EmployeeId &&
                    b.LeaveTypeId == request.LeaveTypeId);

            if (balance == null)
                throw new InvalidOperationException("Leave balance not found.");

            if (balance.LeaveType.Code == "SL")
                await RecalculateSickLeaveAsync(request.EmployeeId);

            if (request.TakenDays > balance.AvailableDays)
                throw new InvalidOperationException(
                    "Used days cannot exceed available days.");

            balance.TakenDays = request.TakenDays;

            if (balance.LeaveType.Code == "AL")
            {
                balance.AvailableDays =
                    balance.CarryoverDays +
                    balance.AccruedDays -
                    balance.TakenDays;
            }
            else
            {
                balance.AvailableDays =
                    balance.AccruedDays - balance.TakenDays;
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
        /// <summary>
        /// Recalculates the annual leave balance for an employee based on their accrual history and any changes to their position or job grade. 
        /// This method is typically called after a position change or at the end of the year to ensure the annual leave balance is accurate. 
        /// It calculates the total accrued days based on the employee's accrual segments, applies any carryover from the previous year, and updates the available days accordingly.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task RecalculateAnnualLeaveAsync(string employeeId)
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

            var cycleStart = employee.StartDate.Year == today.Year
                ? employee.StartDate
                : new DateOnly(today.Year, 1, 1);

            var segments = await _context.EmployeeAccrualRateHistories
                .Where(x => x.EmployeeId == employeeId)
                .OrderBy(x => x.EffectiveFrom)
                .ToListAsync();

            if (segments.Count == 0)
                return;

            decimal totalAccrued = 0m;

            foreach (var segment in segments)
            {
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

            balance.AccruedDays = totalAccrued;

            balance.AvailableDays =
                balance.CarryoverDays +
                totalAccrued -
                balance.TakenDays;

            balance.LastCalculatedDate = today;

            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Recalculates the sick leave balance for an employee based on their tenure and the sick leave policy.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task RecalculateSickLeaveAsync(string employeeId)
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

            decimal AccruedDays;

            if (monthsWorked < 6)
            {
                AccruedDays = monthsWorked;
            }
            else
            {
                AccruedDays = 30;
            }

            var cycleNumber = monthsWorked / 36;

            if (balance.LastResetYear == null || balance.LastResetYear != cycleNumber)
            {
                balance.TakenDays = 0;
                balance.LastResetYear = cycleNumber;
            }

            balance.AccruedDays = AccruedDays;
            balance.AvailableDays = Math.Max(0, AccruedDays - balance.TakenDays);

            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Recalculates the family responsibility leave balance for an employee based on their work anniversary.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task RecalculateFamilyResponsibilityLeaveAsync(string employeeId)
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

            if (today < anniversaryThisYear)
                anniversaryThisYear = anniversaryThisYear.AddYears(-1);

            var anniversaryYear = anniversaryThisYear.Year;

            if (frlBalance.LastResetYear == null ||
                frlBalance.LastResetYear != anniversaryYear)
            {
                frlBalance.TakenDays = 0;
                frlBalance.AccruedDays = 3;
                frlBalance.AvailableDays = 3;
                frlBalance.LastResetYear = anniversaryYear;

                await _context.SaveChangesAsync();
            }
        }
        /// <summary>
        /// Resets the maternity leave balance for an employee when they have a new pregnancy.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task ResetMaternityLeaveForNewPregnancy(string employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            if (employee.Gender != Gender.Female)
                throw new InvalidOperationException("Maternity leave applies to female employees only.");

            var mlBalance = employee.LeaveBalances
                .FirstOrDefault(b => b.LeaveType.Code == "ML");

            if (mlBalance == null)
                throw new InvalidOperationException("Maternity Leave not configured.");

            mlBalance.TakenDays = 0;
            mlBalance.AccruedDays = 120;
            mlBalance.AvailableDays = 120;

            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Projects the annual leave balance for an employee as of a future date based on their accrual history and applicable entitlement rules.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="projectionDate"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<LeaveProjectionResponse> ProjectAnnualLeaveAsync(string employeeId, DateOnly projectionDate)
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

            if (projectionDate <= today)
            {
                return new LeaveProjectionResponse
                {
                    EmployeeName = $"{employee.Name} {employee.Surname}",
                    ProjectionDate = projectionDate,
                    ProjectedAccruedDays = balance.AccruedDays,
                    TakenDays = balance.TakenDays,
                    ProjectedAvailableDays = balance.AvailableDays,
                    DaysWorked = totalDaysWorked
                };
            }

            var rules = await _context.LeaveEntitlementRules
                .Where(r =>
                    r.LeaveTypeId == annualLeave.Id &&
                    r.JobGradeId == employee.Position.JobGradeId &&
                    r.IsActive)
                .OrderBy(r => r.MinYearsService)
                .ToListAsync();

            decimal projectedAvailable = balance.AvailableDays;
            decimal projectedEntitled = balance.AccruedDays;
            decimal projectedCarryover = balance.CarryoverDays;

            var currentDate = today;

            while (currentDate <= projectionDate)
            {
                var yearEnd = new DateOnly(currentDate.Year, 12, 31);

                var periodStart = currentDate;
                var periodEnd = projectionDate < yearEnd ? projectionDate : yearEnd;

                decimal yearsOfService =
                    (periodStart.DayNumber - employee.StartDate.DayNumber) / 365.25m;

                var rule = rules.First(r =>
                    r.MinYearsService <= yearsOfService &&
                    (r.MaxYearsService == null || r.MaxYearsService >= yearsOfService));

                int workingDays = WorkingDayCalculator.CountWorkingDays(
                    periodStart,
                    periodEnd);

                decimal dailyRate =
                    Math.Round((rule.DaysAllocated / 12m) / 21.67m, 6);

                decimal accrued = workingDays * dailyRate;

                projectedEntitled += accrued;

                if (projectedEntitled > rule.DaysAllocated)
                    projectedEntitled = rule.DaysAllocated;

                projectedAvailable =
                    projectedEntitled +
                    projectedCarryover -
                    balance.TakenDays;

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
                EmployeeName = $"{employee.Name} {employee.Surname}",
                ProjectionDate = projectionDate,
                ProjectedAccruedDays = projectedEntitled + projectedCarryover,
                TakenDays = balance.TakenDays,
                ProjectedAvailableDays = projectedAvailable,
                DaysWorked = totalDaysWorked
            };
        }
        /// <summary>
        /// Calculates the years of service for an employee based on their start date.
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        private decimal CalculateYearsOfService(DateOnly startDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (startDate > today)
                return 0;

            var totalDays = today.DayNumber - startDate.DayNumber;
            return Math.Round(totalDays / 365.25m, 2);
        }
        /// <summary>
        /// Calculates the carryover amount for annual leave based on the remaining balance at the end of the year.
        /// </summary>
        /// <param name="remaining"></param>
        /// <returns></returns>
        private decimal CalculateCarryover(decimal remaining)
        {
            if (remaining <= 0)
                return 0;

            return remaining <= 5 ? remaining : 5;
        }
        /// <summary>
        /// Backfills historical annual leave accrual for an employee based on their start date and the applicable entitlement rules.
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        private async Task BackfillHistoricalAnnualAccrualAsync(Employee employee)
        {
            var today = DateTime.UtcNow.Date;
            var currentYear = today.Year;

            if (employee.StartDate.Year >= currentYear)
                return;

            var annualLeave = await _context.LeaveTypes
                .FirstAsync(l => l.Code == "AL" && l.IsActive);

            var balance = await _context.EmployeeLeaveBalances
                .FirstAsync(b =>
                    b.EmployeeId == employee.EmployeeId &&
                    b.LeaveTypeId == annualLeave.Id);

            await _context.Entry(employee)
                .Reference(e => e.Position)
                .LoadAsync();

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

            balance.CarryoverDays = carryover;
            balance.TakenDays = 0;
            balance.LastResetYear = currentYear;
        }
        /// <summary>
        /// Creates an initial accrual segment for an employee if none exist, based on their start date and the applicable entitlement rules.
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        private async Task CreateInitialAccrualSegmentAsync(Employee employee)
        {
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

            await _context.Entry(employee)
                .Reference(e => e.Position)
                .LoadAsync();

            await _context.Entry(employee.Position)
                .Reference(p => p.JobGrade)
                .LoadAsync();

            await _context.EmployeeAccrualRateHistories.AddAsync(
                new EmployeeAccrualRateHistory
                {
                    EmployeeId = employee.EmployeeId,
                    PositionId = employee.PositionId,
                    PositionName = employee.Position.PositionTitle,
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
#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
namespace HRConnect.Api.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.Utils;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Utils;
    using System.Globalization;

    public class LeaveBalanceService : ILeaveBalanceService
    {
        private readonly ApplicationDBContext _context;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ILeaveTypeRepository _leaveTypeRepo;
        private readonly IEmployeeLeaveBalanceRepository _employeeLeaveBalRepo;

        public LeaveBalanceService(ApplicationDBContext context, IEmployeeRepository employeeRepo, ILeaveTypeRepository leaveTypeRepo, IEmployeeLeaveBalanceRepository employeeLeaveBalRepo)
        {
            _context = context;
            _employeeLeaveBalRepo = employeeLeaveBalRepo;
            _leaveTypeRepo = leaveTypeRepo;
            _employeeRepo = employeeRepo;
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
            var employee = await _employeeRepo.GetEmployeeWithLeaveBalancesAsync(employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            if (employee.Position == null)
                throw new InvalidOperationException("Employee position not found.");
            if (employee.Position == null)
                throw new InvalidOperationException("Employee position not found.");

            var groupKey = await _employeeLeaveBalRepo.GetGroupKeyByJobGradeIdAsync(employee.Position.JobGradeId);

            if (groupKey == null)
                throw new InvalidOperationException("JobGrade not mapped to any group.");

            var yearsOfService =
    CalculateYearsOfService.UsingStartDate(employee.StartDate);

            var leaveTypes = await _employeeLeaveBalRepo.GetActiveLeaveTypesAsync();


            var balancesToAdd = new List<EmployeeLeaveBalance>();

            foreach (var leaveType in leaveTypes)
            {
                if (leaveType.FemaleOnly && employee.Gender != Gender.Female)
                    continue;

                if (employee.LeaveBalances.Any(b => b.LeaveTypeId == leaveType.Id))
                    continue;

                if (leaveType.Code == "AL")
                {

                    var rule = await _employeeLeaveBalRepo.GetApplicableLeaveRuleAsync(leaveType.Id, groupKey, yearsOfService);


                    if (rule == null)
                        continue;

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

                    await _employeeLeaveBalRepo.AddLeaveBalanceAsync(annualBalance);
                    await _context.SaveChangesAsync();

                    // Backfill accrual history
                    await BackfillHistoricalAnnualAccrualAsync(employee);

                    // Ensure accrual segment exists
                    var hasSegment = await _employeeLeaveBalRepo.HasAccrualRateHistoryAsync(employee.EmployeeId);

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
                        AccruedDays = 30,
                        TakenDays = 0,
                        AvailableDays = 30
                    };

                    await _employeeLeaveBalRepo.AddLeaveBalanceAsync(sickBalance);
                    await _employeeLeaveBalRepo.SaveChangesAsync();

                    await RecalculateSickLeaveAsync(employee.EmployeeId);
                    continue;
                }

                if (leaveType.Code == "FRL")
                {
                    var frlBalance = new EmployeeLeaveBalance
                    {
                        EmployeeId = employee.EmployeeId,
                        LeaveTypeId = leaveType.Id,
                        AccruedDays = 3,
                        TakenDays = 0,
                        AvailableDays = 3,
                        LastResetYear = DateTime.UtcNow.Year
                    };

                    await _employeeLeaveBalRepo.AddLeaveBalanceAsync(frlBalance);
                    await _employeeLeaveBalRepo.SaveChangesAsync();

                    await RecalculateFamilyResponsibilityLeaveAsync(employee.EmployeeId);
                    continue;
                }
                if (leaveType.Code == "ML")
                {
                    var maternityBalance = new EmployeeLeaveBalance
                    {
                        EmployeeId = employee.EmployeeId,
                        LeaveTypeId = leaveType.Id,
                        AccruedDays = 120,
                        TakenDays = 0,
                        AvailableDays = 120
                    };

                    balancesToAdd.Add(maternityBalance);

                    continue;
                }

                var defaultBalance = new EmployeeLeaveBalance
                {
                    EmployeeId = employee.EmployeeId,
                    LeaveTypeId = leaveType.Id,
                    AccruedDays = 0,
                    TakenDays = 0,
                    AvailableDays = 0
                };

                await _employeeLeaveBalRepo.AddLeaveBalancesAsync(balancesToAdd);
                await _employeeLeaveBalRepo.SaveChangesAsync();
                await BackfillHistoricalAnnualAccrualAsync(employee);
                await CreateInitialAccrualSegmentAsync(employee);
            }
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
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.EmployeeId))
                throw new ArgumentException("EmployeeId is required.");

            if (request.LeaveTypeId <= 0)
                throw new ArgumentException("Invalid LeaveTypeId.");

            if (request.TakenDays <= 0)
                throw new InvalidOperationException("Taken days must be greater than zero.");


            var balance = await _employeeLeaveBalRepo.GetLeaveBalanceAsync(request.EmployeeId, request.LeaveTypeId);

            if (balance == null)
                throw new InvalidOperationException("Leave balance not found.");

            if (balance.LeaveType == null)
                throw new InvalidOperationException("Leave type is not loaded for the balance.");

            if (balance.LeaveType.Code == "SL")
                await RecalculateSickLeaveAsync(request.EmployeeId);

            decimal totalEntitlement;

            if (balance.LeaveType.Code == "ML")
            {
                totalEntitlement = balance.LeaveType.EntitlementRules
                    .Where(r => r.IsActive && r.GroupKey == "ALL")
                    .Select(r => r.DaysAllocated)
                    .FirstOrDefault();
            }
            else
            {
                totalEntitlement = balance.AccruedDays + balance.CarryoverDays;
            }

            if (balance.TakenDays + request.TakenDays > totalEntitlement)
                throw new InvalidOperationException("Total taken days exceed entitlement.");

            balance.TakenDays += request.TakenDays;

            if (balance.LeaveType.Code == "ML")
            {
                balance.AvailableDays = totalEntitlement - balance.TakenDays;
            }
            else if (balance.LeaveType.Code == "AL")
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

            if (balance.AvailableDays < 0)
                balance.AvailableDays = 0;

            try
            {
                await _employeeLeaveBalRepo.SaveChangesAsync();
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

            var employee = await _employeeLeaveBalRepo.GetEmployeeForAnnualLeaveAsync(employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            var annualLeave = await _employeeLeaveBalRepo.GetActiveAnnualLeaveTypeAsync();

            var balance = employee.LeaveBalances?
                .FirstOrDefault(b => b.LeaveTypeId == annualLeave.Id)
                ?? throw new InvalidOperationException("Annual leave balance not found.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var cycleStart = employee.StartDate.Year == today.Year
                ? employee.StartDate
                : new DateOnly(today.Year, 1, 1);

            var segments = await _employeeLeaveBalRepo.GetEmployeeAccrualRateHistoriesAsync(employeeId);

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

            await _employeeLeaveBalRepo.SaveChangesAsync();
        }
        /// <summary>
        /// Recalculates the sick leave balance for an employee based on their tenure and the sick leave policy.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task RecalculateSickLeaveAsync(string employeeId)
        {

            var employee = await _employeeLeaveBalRepo.GetEmployeeWithLeaveBalancesAsync(employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            var sickLeave = await _leaveTypeRepo.GetActiveLeaveTypeByCodeAsync("SL");



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

            await _employeeLeaveBalRepo.SaveChangesAsync();
        }
        /// <summary>
        /// Recalculates the family responsibility leave balance for an employee based on their work anniversary.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task RecalculateFamilyResponsibilityLeaveAsync(string employeeId)
        {

            var employee = await _employeeLeaveBalRepo.GetEmployeeWithLeaveBalancesAsync(employeeId);

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

                await _employeeLeaveBalRepo.SaveChangesAsync();
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

            var employee = await _employeeLeaveBalRepo.GetEmployeeWithLeaveBalancesAsync(employeeId);

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

            await _employeeLeaveBalRepo.SaveChangesAsync();
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

            var employee = await _employeeLeaveBalRepo.GetEmployeeForAnnualLeaveAsync(employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            var annualLeave = await _employeeLeaveBalRepo.GetActiveAnnualLeaveTypeAsync();

            var balance = employee.LeaveBalances?
                .FirstOrDefault(b => b.LeaveTypeId == annualLeave.Id)
                ?? throw new InvalidOperationException("Annual leave balance not found.");

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

            var groupKey = await _employeeLeaveBalRepo.GetGroupKeyByJobGradeIdAsync(employee.Position.JobGradeId);

            if (groupKey == null)
                throw new InvalidOperationException("JobGrade not mapped.");

            var rules = await _employeeLeaveBalRepo.GetAnnualLeaveRulesAsync(annualLeave.Id, groupKey);

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
                (r.MaxYearsService == null || yearsOfService < r.MaxYearsService));

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

            if (employee.Position == null)
                throw new InvalidOperationException("Employee position not found.");

            var groupKey = await _employeeLeaveBalRepo.GetGroupKeyByJobGradeIdAsync(employee.Position.JobGradeId);

            if (groupKey == null)
                throw new InvalidOperationException("JobGrade not mapped.");

            var annualLeave = await _employeeLeaveBalRepo.GetActiveAnnualLeaveTypeAsync();

            var balance = await _employeeLeaveBalRepo.GetEmployeeLeaveBalanceAsync(employee.EmployeeId, annualLeave.Id);

            await _context.Entry(employee)
                .Reference(e => e.Position)
                .LoadAsync();

            var endOfPreviousYearDate = new DateTime(currentYear - 1, 12, 31);

            var yearsOfService = (decimal)((endOfPreviousYearDate - employee.StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.25);

            var rule = await _employeeLeaveBalRepo.GetApplicableLeaveRuleAsync(annualLeave.Id, groupKey, yearsOfService);

            decimal accrued = rule.DaysAllocated;

            var carryover = accrued <= 5 ? accrued : 5;
            var forfeited = accrued > 5 ? accrued - 5 : 0;

            var yearToClose = currentYear - 1;


            var alreadyExists = await _leaveTypeRepo.AnnualLeaveHistoryExistsAsync(employee.EmployeeId, yearToClose);

            if (!alreadyExists)
            {
                var history =
                    new AnnualLeaveAccrualHistory
                    {
                        EmployeeId = employee.EmployeeId,
                        Year = yearToClose,
                        Accrued = accrued,
                        Forfeited = forfeited,
                        ClosingBalance = carryover,
                        CreatedDate = DateTime.UtcNow
                    };

                await _leaveTypeRepo.AddAnnualLeaveAccrualHistoryAsync(history);

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
            var exists = await _employeeLeaveBalRepo.HasAccrualRateHistoryAsync(employee.EmployeeId);

            if (exists)
                return;

            if (employee.Position == null)
                throw new InvalidOperationException("Employee position not found.");


            var groupKey = await _employeeLeaveBalRepo.GetGroupKeyByJobGradeIdAsync(employee.Position.JobGradeId);

            if (groupKey == null)
                throw new InvalidOperationException("JobGrade not mapped.");

            var annualLeave = await _employeeLeaveBalRepo.GetActiveAnnualLeaveTypeAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var yearsOfService =
                today.Year - employee.StartDate.Year;

            if (today < employee.StartDate.AddYears(yearsOfService))
            {
                yearsOfService--;
            }

            var rule = await _employeeLeaveBalRepo.GetHistoricalAnnualLeaveRuleAsync(annualLeave.Id, groupKey, yearsOfService);

            await CreateAccrualSegmentAsync(
                employee,
                rule.DaysAllocated,
                "Initial Accrual",
                employee.StartDate);
        }
        public async Task RecalculateFamilyResponsibilityLeaveBulkAsync(List<string> employeeIds)
        {
            foreach (var id in employeeIds)
            {
                await RecalculateFamilyResponsibilityLeaveAsync(id);
            }
        }
        public async Task RecalculateAnnualLeaveBulkAsync(List<string> employeeIds)
        {
            foreach (var id in employeeIds)
            {
                await RecalculateAnnualLeaveAsync(id);
            }
        }
        public async Task CreateAccrualSegmentAsync(Employee employee, decimal annualEntitlement, string reason, DateOnly effectiveFrom)
        {
            var currentSegment = await _employeeLeaveBalRepo.GetCurrentAccrualSegmentAsync(employee.EmployeeId);

            if (currentSegment != null)
            {
                if (currentSegment.EffectiveFrom == effectiveFrom)
                {
                    _employeeLeaveBalRepo.RemoveAccrualRateHistory(currentSegment);
                }
                else
                {
                    currentSegment.EffectiveTo =
                        effectiveFrom.AddDays(-1);
                }
            }

            await _employeeLeaveBalRepo.AddAccrualRateHistoryAsync(
                new EmployeeAccrualRateHistory
                {
                    EmployeeId = employee.EmployeeId,
                    PositionId = employee.PositionId,
                    PositionName = employee.Position?.PositionTitle
                        ?? "Unknown",

                    AnnualEntitlement = annualEntitlement,

                    DailyRate =
                        (annualEntitlement / 12m) / 21.67m,

                    EffectiveFrom = effectiveFrom,

                    CreatedDate = DateTime.UtcNow,

                    Reason = reason
                });

            await _employeeLeaveBalRepo.SaveChangesAsync();
        }
        public async Task CheckYearsOfServiceAccrualChangeAsync(string employeeId)
        {

            var employee = await _employeeLeaveBalRepo.GetEmployeeWithLeaveBalancesAsync(employeeId);

            if (employee == null)
                throw new InvalidOperationException(
                    "Employee not found.");

            if (employee.Position == null)
                throw new InvalidOperationException(
                    "Employee position not found.");

            
            var groupKey = await _employeeLeaveBalRepo.GetGroupKeyByJobGradeIdAsync(employee.Position.JobGradeId);

            if (groupKey == null)
                throw new InvalidOperationException(
                    "JobGrade not mapped.");
            
            var annualLeave = await _employeeLeaveBalRepo.GetActiveAnnualLeaveTypeAsync();

            var yearsOfService =
                CalculateYearsOfService.UsingStartDate(employee.StartDate);
            
            var applicableRule = await _employeeLeaveBalRepo.GetApplicableLeaveRuleAsync(annualLeave.Id, groupKey, yearsOfService);

            if (applicableRule == null)
                return;

            var currentSegment =
                await _employeeLeaveBalRepo.GetCurrentAccrualSegmentAsync(employeeId);

            if (currentSegment == null)
                return;

            var entitlementChanged = currentSegment.AnnualEntitlement != applicableRule.DaysAllocated;

            if (!entitlementChanged)
            {
                return;
            }
            await CreateAccrualSegmentAsync(
                employee,
                applicableRule.DaysAllocated,
                "Years Of Service Change",
                DateOnly.FromDateTime(DateTime.UtcNow));

            await RecalculateAnnualLeaveAsync(employeeId);
        }

        public async Task ApplyEntitlementRuleChangesAsync()
        {
            
            var employees = await _employeeRepo.GetEmployeesWithPositionsAsync();

            var annualLeave = await _employeeLeaveBalRepo.GetActiveAnnualLeaveTypeAsync();

            if (annualLeave == null)
                return;

            foreach (var employee in employees)
            {
                if (employee.Position == null)
                    continue;

                var groupKey = await _employeeLeaveBalRepo.GetGroupKeyByJobGradeIdAsync(employee.Position.JobGradeId);

                if (groupKey == null)
                    continue;

                var yearsOfService =
                    CalculateYearsOfService.UsingStartDate(employee.StartDate);


                var applicableRule =
                    await _employeeLeaveBalRepo.GetApplicableLeaveRuleAsync(annualLeave.Id, groupKey, yearsOfService);

                if (applicableRule == null)
                    continue;
                
                var currentSegment =
                    await _employeeLeaveBalRepo.GetCurrentAccrualSegmentAsync(employee.EmployeeId);

                if (currentSegment == null)
                    continue;

                if (currentSegment.AnnualEntitlement ==
                    applicableRule.DaysAllocated)
                {
                    continue;
                }

                await CreateAccrualSegmentAsync(
                    employee,
                    applicableRule.DaysAllocated,
                    "Entitlement Rule Change",
                    DateOnly.FromDateTime(DateTime.UtcNow));

                await RecalculateAnnualLeaveAsync(
                    employee.EmployeeId);
            }
        }
    }
}
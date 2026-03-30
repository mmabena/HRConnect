namespace HRConnect.Api.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Utils;
    public class LeaveProcessingService : ILeaveProcessingService
    {
        private readonly ApplicationDBContext _context;
        private readonly IEmailService _emailService;
        private readonly ILeaveBalanceService _leaveBalanceService;

        public LeaveProcessingService(
            ApplicationDBContext context,
            IEmailService emailService,
            ILeaveBalanceService leaveBalanceService)
        {
            _context = context;
            _emailService = emailService;
            _leaveBalanceService = leaveBalanceService;
        }
        /// <summary>
        /// Recalculates the sick leave balance for all employees based on their tenure and the sick leave policy.
        /// </summary>
        /// <returns></returns>
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

                if (monthsWorked >= 36)
                {
                    sickBalance.TakenDays = 0;
                    entitled = 30;
                }

                sickBalance.AccruedDays = entitled;
                sickBalance.AvailableDays =
                    Math.Max(0, entitled - sickBalance.TakenDays);
            }

            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Recalculates the family responsibility leave balance for all employees based on their work anniversary and the applicable policy.
        /// </summary>
        /// <returns></returns>
        public async Task RecalculateAllFamilyResponsibilityLeaveAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .ToListAsync();

            foreach (var employee in employees)
            {
                await _leaveBalanceService
                    .RecalculateFamilyResponsibilityLeaveAsync(employee.EmployeeId);
            }
        }
        /// <summary>
        /// Resets the maternity leave balance for all eligible employees when they have a new pregnancy, based on the applicable policy.
         /// </summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task ProcessCarryOverNotificationAsync()
        {
            var today = DateTime.UtcNow.Date;

            if (today.Month != 12 || today.Day != 1)
            {
                return;
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

                var body = EmailTemplates.GenerateCarryOverWarningEmail(
                    balance.Employee,
                    balance.AvailableDays,
                    forfeited
                );

                await _emailService.SendEmailAsync(
                    balance.Employee.Email,
                    subject,
                    body
                );
            }
        }
        /// <summary>
        /// Processes the annual leave reset for all employees at the end of the year, applying the carryover policy and recording the accrual history.
        /// </summary>
        /// <param name="overrideYear"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task ProcessAnnualResetAsync(int? overrideYear = null)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var today = DateTime.UtcNow.Date;

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
                    if (balance.LastResetYear == currentYear)
                        continue;

                    var yearToClose = currentYear - 1;

                    var openingBalance = balance.CarryoverDays;
                    var accrued = balance.AccruedDays;
                    var used = balance.TakenDays;

                    var closingBalance = openingBalance + accrued - used;

                    var carryoverApplied = CalculateCarryover(closingBalance);

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

                    balance.CarryoverDays = carryoverApplied;
                    balance.ForfeitedDays = 0;
                    balance.AccruedDays = 0;
                    balance.AvailableDays = carryoverApplied;
                    balance.TakenDays = 0;
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
        /// <summary>
        /// Calculates the carryover amount for annual leave based on the remaining balance at the end of the year, applying the policy of capping carryover at 5 days.
        /// </summary>
        /// <param name="remaining"></param>
        /// <returns></returns>
        private decimal CalculateCarryover(decimal remaining)
        {
            if (remaining <= 0)
                return 0;

            return remaining <= 5 ? remaining : 5;
        }
    }
}
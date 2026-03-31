namespace HRConnect.Api.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Utils;

    public class LeaveRuleService : ILeaveRuleService
    {
        private readonly ApplicationDBContext _context;
        private readonly IEmailService _emailService;
        private readonly ILeaveBalanceService _leaveBalanceService;

        public LeaveRuleService(
            ApplicationDBContext context,
            IEmailService emailService,
            ILeaveBalanceService leaveBalanceService)
        {
            _context = context;
            _emailService = emailService;
            _leaveBalanceService = leaveBalanceService;
        }
        /// <summary>
        /// Updates the leave entitlement rule and recalculates the leave balances for all affected employees, sending notification emails about the change.
        /// The method validates the input, checks for any conflicts with existing leave balances, and applies the new entitlement to all employees who fall under the rule's criteria, ensuring that no employee's entitlement is reduced below their already taken days.
        /// After updating the rule, it triggers a recalculation of leave balances for all affected employees and sends them an email notification about the change in their leave policy.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.LeaveBalances)
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

                if (request.NewDaysAllocated < balance.TakenDays)
                    throw new InvalidOperationException(
                        $"Cannot reduce entitlement below used days for employee {employee.Name}.");
            }

            rule.DaysAllocated = request.NewDaysAllocated;

            await _context.SaveChangesAsync();

            await RecalculateEmployeesForRuleChangeAsync(rule.Id);
        }
        /// <summary>
        /// Recalculates the leave balances for all employees affected by a change in a leave entitlement rule, 
        /// based on their tenure and the new entitlement, and sends notification emails about the change.  
        /// </summary>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task RecalculateEmployeesForRuleChangeAsync(int ruleId)
        {
            var rule = await _context.LeaveEntitlementRules
                .Include(r => r.LeaveType)
                .FirstOrDefaultAsync(r => r.Id == ruleId);

            if (rule == null)
                throw new InvalidOperationException("Rule not found.");

            var employees = await _context.Employees
                .Include(e => e.Position)
                .Include(e => e.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .Where(e => e.Position.JobGradeId == rule.JobGradeId)
                .ToListAsync();

            var employeeIds = employees.Select(e => e.EmployeeId).ToList();

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

                var segment = segments
                    .FirstOrDefault(x => x.EmployeeId == employee.EmployeeId);

                if (segment != null)
                {
                    segment.AnnualEntitlement = rule.DaysAllocated;
                    segment.DailyRate = rule.DaysAllocated / 260m;
                }

                await _leaveBalanceService.RecalculateAnnualLeaveAsync(employee.EmployeeId);

                var updatedBalance = employee.LeaveBalances
                    .First(lb => lb.LeaveTypeId == rule.LeaveTypeId);

                var emailBody = EmailTemplates.GenerateRuleChangeEmail(
                     employee,
                     rule.DaysAllocated,
                     updatedBalance.AvailableDays
                 );

                await _emailService.SendEmailAsync(
                    employee.Email,
                    "Leave Policy Updated",
                    emailBody
                );
            }

            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// Calculates the years of service for an employee based on their start date, 
        /// used for determining leave entitlements under different rules. 
        /// The calculation accounts for leap years by using an average year length of 365.25 days.
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
    }
}
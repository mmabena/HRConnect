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
        private readonly ILeaveTypeRepository _leaveTypeRepo;
        private readonly ILeaveBalanceService _leaveBalanceService;

        public LeaveRuleService(
            ApplicationDBContext context,
            IEmailService emailService,
            ILeaveTypeRepository leaveTypeRepo,
            ILeaveBalanceService leaveBalanceService)
        {
            _context = context;
            _emailService = emailService;
            _leaveTypeRepo = leaveTypeRepo;
            _leaveBalanceService = leaveBalanceService;
        }

        public async Task UpdateLeaveEntitlementRuleAsync(UpdateLeaveRuleRequest request)
        {
            if (request.NewDaysAllocated < 0)
                throw new InvalidOperationException("Days allocated cannot be negative.");

            var rule = await _leaveTypeRepo.GetLeaveRuleWithLeaveTypeAsync(request.RuleId);

            if (rule == null)
                throw new InvalidOperationException("Rule not found.");

            if (rule.MinYearsService < 0)
                throw new InvalidOperationException("MinYearsService cannot be negative.");

            if (rule.MaxYearsService.HasValue &&
                rule.MaxYearsService < rule.MinYearsService)
                throw new InvalidOperationException("MaxYearsService cannot be less than MinYearsService.");

            var employees = await _leaveTypeRepo.GetEmployeesForLeaveRuleAsync(rule.GroupKey);

            foreach (var employee in employees)
            {
                var yearsOfService = CalculateYearsOfService.UsingStartDate(employee.StartDate);

                if (yearsOfService < rule.MinYearsService)
                    continue;

                if (rule.MaxYearsService.HasValue &&
                    yearsOfService >= rule.MaxYearsService.Value)
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

            await _leaveTypeRepo.UpdateLeaveRuleAsync(rule);

            await RecalculateEmployeesForRuleChangeAsync(rule.Id);
        }

        public async Task RecalculateEmployeesForRuleChangeAsync(int ruleId)
        {
            var rule = await _leaveTypeRepo.GetLeaveRuleWithLeaveTypeAsync(ruleId);

            if (rule == null)
                throw new InvalidOperationException("Rule not found.");

            var employees = await _leaveTypeRepo.GetEmployeesForLeaveRuleAsync(rule.GroupKey);

            var employeeIds = employees.Select(e => e.EmployeeId).ToList();

            var segments = await _leaveTypeRepo.GetActiveAccrualRateHistoriesAsync(employeeIds);

            foreach (var employee in employees)
            {
                var yearsOfService =
    CalculateYearsOfService.UsingStartDate(employee.StartDate);

                if (yearsOfService < rule.MinYearsService)
                    continue;

                if (rule.MaxYearsService.HasValue &&
                    yearsOfService >= rule.MaxYearsService.Value)
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
                    segment.DailyRate = (rule.DaysAllocated / 12m) / 21.67m;
                }

                await _leaveBalanceService.RecalculateAnnualLeaveAsync(employee.EmployeeId);

                var updatedBalance = employee.LeaveBalances
                    .FirstOrDefault(lb => lb.LeaveTypeId == rule.LeaveTypeId);

                if (updatedBalance == null)
                    continue;

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

            await _leaveTypeRepo.SaveChangesAsync();
        }
    }
}
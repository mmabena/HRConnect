namespace HRConnect.Api.Services
{
    using HRConnect.Api.Data;
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

        // ================================
        // INITIALIZATION
        // ================================
        public async Task InitializeEmployeeLeaveBalancesAsync(Guid employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Position)
                .ThenInclude(p => p.JobGrade)
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

                //Duplicate Protection
                var alreadyExists = employee.LeaveBalances
                .Any(b => b.LeaveTypeId == leaveType.Id);

                if (alreadyExists)
                    continue;

                var rule = await _context.LeaveEntitlementRules
                    .Where(r =>
                        r.LeaveTypeId == leaveType.Id &&
                        r.JobGradeId == employee.Position.JobGradeId &&
                        r.MinYearsService <= yearsOfService &&
                        (r.MaxYearsService == null || r.MaxYearsService >= yearsOfService) &&
                        r.IsActive)
                    .FirstOrDefaultAsync();

                if (rule == null)
                    continue;

                var balance = new EmployeeLeaveBalance
                {
                    EmployeeId = employee.EmployeeId,
                    LeaveTypeId = leaveType.Id,
                    EntitledDays = rule.DaysAllocated,
                    AccruedDays = 0,
                    UsedDays = 0,
                    RemainingDays = rule.DaysAllocated
                };

                await _context.EmployeeLeaveBalances.AddAsync(balance);
            }

            await _context.SaveChangesAsync();
        }

        // ================================
        // ANNUAL RECALCULATION
        // ================================
        public async Task RecalculateAnnualLeaveAsync(Guid employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Position)
                .ThenInclude(p => p.JobGrade)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            if (!employee.UpdatedDate.HasValue)
                throw new InvalidOperationException("UpdatedDate not set for employee.");

            var changeDate = DateOnly.FromDateTime(employee.UpdatedDate.Value);

            var annualLeave = await _context.LeaveTypes
                .FirstOrDefaultAsync(l => l.Code == "AL" && l.IsActive);

            if (annualLeave == null)
                throw new InvalidOperationException("Annual Leave type not configured.");

            var balance = await _context.EmployeeLeaveBalances
                .FirstOrDefaultAsync(b =>
                    b.EmployeeId == employeeId &&
                    b.LeaveTypeId == annualLeave.Id);

            if (balance == null)
                throw new InvalidOperationException("Annual leave balance not found.");

            // OLD annual allocation (before promotion)
            var oldAnnualAllocation = balance.EntitledDays;

            var yearsOfService = CalculateYearsOfService(employee.StartDate);

            // NEW rule based on new JobGrade
            var newRule = await _context.LeaveEntitlementRules
                .Where(r =>
                    r.LeaveTypeId == annualLeave.Id &&
                    r.JobGradeId == employee.Position.JobGradeId &&
                    r.MinYearsService <= yearsOfService &&
                    (r.MaxYearsService == null || r.MaxYearsService >= yearsOfService) &&
                    r.IsActive)
                .FirstOrDefaultAsync();

            if (newRule == null)
                throw new InvalidOperationException("New annual rule not found.");

            var newAnnualAllocation = newRule.DaysAllocated;

            int monthsBeforeChange = changeDate.Month - 1;
            int monthsAfterChange = 12 - monthsBeforeChange;

            decimal entitlementBefore = (oldAnnualAllocation / 12m) * monthsBeforeChange;
            decimal entitlementAfter = (newAnnualAllocation / 12m) * monthsAfterChange;

            var recalculatedTotal = Math.Round(entitlementBefore + entitlementAfter, 2);

            balance.EntitledDays = recalculatedTotal;
            balance.RemainingDays = recalculatedTotal - balance.UsedDays;

            await _context.SaveChangesAsync();
        }

        // ================================
        // YEARS CALCULATION
        // ================================
        private decimal CalculateYearsOfService(DateOnly startDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var totalDays = today.DayNumber - startDate.DayNumber;
            return Math.Round(totalDays / 365.25m, 2);
        }
    }
}

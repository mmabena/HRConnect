namespace HRConnect.Api.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.Utils;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Utils;
    using Microsoft.AspNetCore.SignalR;
    using HRConnect.Api.Hubs;
    public class LeaveProcessingService : ILeaveProcessingService
    {
        private readonly ApplicationDBContext _context;
        private readonly IEmailService _emailService;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ILeaveTypeRepository _leaveTypeRepo;
        private readonly ILeaveBalanceService _leaveBalanceService;
        private readonly IHubContext<LeaveHub> _hubContext;

        public LeaveProcessingService(
            ApplicationDBContext context,
            IEmailService emailService, IEmployeeRepository employeeRepo,
            ILeaveBalanceService leaveBalanceService, ILeaveTypeRepository leaveTypeRepo,
            IHubContext<LeaveHub> hubContext)
        {
            _context = context;
            _employeeRepo = employeeRepo;
            _leaveTypeRepo = leaveTypeRepo;
            _emailService = emailService;
            _leaveBalanceService = leaveBalanceService;
            _hubContext = hubContext;
        }
        /// <summary>
        /// Recalculates the sick leave balance for all employees based on their tenure and the sick leave policy.
        /// </summary>
        /// <returns></returns>
        public async Task RecalculateAllSickLeaveAsync()
        {
            var employees = await _employeeRepo.GetEmployeesWithSickLeaveAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            foreach (var employee in employees)
            {
                await _leaveBalanceService.RecalculateSickLeaveAsync(employee.EmployeeId);
            }
        }
        /// <summary>
        /// Recalculates the family responsibility leave balance for all employees based on their work anniversary and the applicable policy.
        /// </summary>
        /// <returns></returns>
        public async Task RecalculateAllFamilyResponsibilityLeaveAsync()
        {
            var employees = await _employeeRepo.GetEmployeesWithFamilyResponsibilityLeaveAsync();

            await _leaveBalanceService
                .RecalculateFamilyResponsibilityLeaveBulkAsync(
                employees.Select(e => e.EmployeeId).ToList()
            );
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

            var annualLeave = await _leaveTypeRepo.GetActiveLeaveTypeByCodeAsync("AL");

            if (annualLeave == null)
                throw new InvalidOperationException("Annual Leave not configured");

            var balances =
              await _leaveTypeRepo.GetAnnualLeaveBalancesAsync(annualLeave.Id);

            var balancesToNotify = balances
                .Where(b => b.AvailableDays > 5)
                .ToList();

            foreach (var balance in balancesToNotify)
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

                var annualLeave = await _leaveTypeRepo.GetActiveLeaveTypeByCodeAsync("AL");

                if (annualLeave == null)
                    throw new InvalidOperationException("Annual Leave not configured.");

                var balances = await _leaveTypeRepo.GetAnnualLeaveBalancesAsync(annualLeave.Id);

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

                    var alreadyExists = await _leaveTypeRepo.AnnualLeaveHistoryExistsAsync(balance.EmployeeId, yearToClose);

                    if (!alreadyExists)
                    {
                        var history =
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
                            };

                        await _leaveTypeRepo.AddAnnualLeaveAccrualHistoryAsync(history);
                    }

                    balance.CarryoverDays = carryoverApplied;
                    balance.ForfeitedDays = 0;
                    balance.AccruedDays = 0;
                    balance.AvailableDays = carryoverApplied;
                    balance.TakenDays = 0;
                    balance.LastResetYear = currentYear;
                }

                await _leaveTypeRepo.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Annual reset failed", ex);
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
        public async Task ProcessExpiredPendingLeaveApplicationsAsync()
        {
            var expiryCutoff = DateTime.UtcNow.AddDays(-2);

            var expiredApplications = await _leaveTypeRepo.GetExpiredPendingLeaveApplicationsAsync(expiryCutoff);

            foreach (var application in expiredApplications)
            {
                var employee = await _employeeRepo.GetEmployeeByIdAsync(application.EmployeeId);

                if (employee == null)
                    continue;

                var leaveType = await _leaveTypeRepo.GetLeaveTypeByIdAsync(application.LeaveTypeId);


                if (leaveType == null)
                    continue;

                application.Status = LeaveApplication.LeaveApplicationStatus.Rejected;

                application.DecisionDate = DateTime.UtcNow;

                application.RejectionReason =
                    "Leave application automatically rejected because it was not reviewed within 2 days.";

                application.DecisionBy = "System Auto-Reject";

                var subject = "Leave Application Rejected";

                var body = EmailTemplates.GenerateDecisionEmailHtml(
                    employee,
                    leaveType,
                    application,
                    false
                );

                await _emailService.SendEmailAsync(
                    employee.Email,
                    subject,
                    body
                );

                await _hubContext.Clients
                    .Group(application.EmployeeId)
                    .SendAsync(
                        "LeaveUpdated",
                        new
                        {
                            employeeId = application.EmployeeId,
                            applicationId = application.Id,
                            status = application.Status.ToString()
                        });
            }
            await _leaveTypeRepo.SaveChangesAsync();
        }
    }
}
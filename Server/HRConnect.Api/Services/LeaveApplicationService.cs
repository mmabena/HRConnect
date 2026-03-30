namespace HRConnect.Api.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.Utils;
    using Microsoft.EntityFrameworkCore;

    public class LeaveApplicationService : ILeaveApplicationService
    {
        private readonly ApplicationDBContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public LeaveApplicationService(
            ApplicationDBContext context,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }
        /// <summary>
        /// Processes a leave application request by validating the employee, leave type, and requested dates, checking the employee's leave balance,
        /// creating a new leave application record, and sending an email notification to the employee's manager for approval, 
        /// while ensuring that leave requests cannot span multiple years and that all necessary validations are performed to maintain data integrity.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LeaveApplicationResponse> ApplyForLeaveAsync(CreateApplicationRequest request)
        {
            var employee = await _context.Employees
                .Include(e => e.LeaveBalances)
                .FirstOrDefaultAsync(e => e.EmployeeId == request.EmployeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            if (request.EndDate < request.StartDate)
                throw new InvalidOperationException("End date cannot be before start date.");

            // Prevent leave requests crossing entitlement cycles
            if (request.StartDate.Year != request.EndDate.Year)
            {
                throw new InvalidOperationException(
                    "Leave requests cannot span multiple years. " +
                    "Please submit separate leave requests for each year.");
            }

            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(l => l.Id == request.LeaveTypeId);

            if (leaveType == null)
                throw new InvalidOperationException("Leave type not found.");

            var balance = employee.LeaveBalances
                .FirstOrDefault(lb => lb.LeaveTypeId == request.LeaveTypeId);

            if (balance == null)
                throw new InvalidOperationException("Leave balance not found.");

            var daysRequested =
                WorkingDayCalculator.CountWorkingDays(request.StartDate, request.EndDate);

            if (balance.AvailableDays < daysRequested)
                throw new InvalidOperationException("Insufficient leave balance.");

            var application = new LeaveApplication
            {
                EmployeeId = request.EmployeeId,
                LeaveTypeId = request.LeaveTypeId,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DaysRequested = daysRequested,
                Status = LeaveApplication.LeaveApplicationStatus.Pending,
                AppliedDate = DateTime.UtcNow
            };

            await _context.LeaveApplications.AddAsync(application);
            await _context.SaveChangesAsync();
            await SendManagerApprovalEmail(application);

            return MapToResponse(application);
        }
        /// <summary>
        /// Approves a pending leave application by validating the application ID and approval token, 
        /// checking that the application is still pending and that the approval link has not expired(takes 2 days to expire),
        /// updating the application status to approved, adjusting the employee's leave balance accordingly, 
        /// and sending an email notification to the employee about the approval decision, 
        /// while ensuring that all necessary validations are performed to maintain data integrity and that only authorized approvals are processed.
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task ApproveLeaveAsync(int applicationId, Guid token)
        {
            var application = await _context.LeaveApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                throw new InvalidOperationException("Leave application not found");

            if (application.ApprovalToken != token)
                throw new InvalidOperationException("Invalid approval token");

            if (application.TokenExpiry < DateTime.UtcNow)
                throw new InvalidOperationException("Approval link expired");

            if (application.Status != LeaveApplication.LeaveApplicationStatus.Pending)
                throw new InvalidOperationException("Only pending applications can be approved");

            var balance = await _context.EmployeeLeaveBalances
                .FirstAsync(b => b.EmployeeId == application.EmployeeId &&
                                 b.LeaveTypeId == application.LeaveTypeId);

            balance.TakenDays += application.DaysRequested;
            balance.AvailableDays -= application.DaysRequested;

            application.Status = LeaveApplication.LeaveApplicationStatus.Approved;
            application.DecisionDate = DateTime.UtcNow;
            var employee = await _context.Employees
                .FirstAsync(e => e.EmployeeId == application.EmployeeId);

            var manager = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employee.CareerManagerID);

            application.DecisionBy = manager != null
                ? $"{manager.Name} {manager.Surname}"
                : "Admin";

            await _context.SaveChangesAsync();

            await SendEmployeeDecisionEmail(application, true);
        }
        public async Task ApproveLeaveInternalAsync(int applicationId)
        {
            var application = await _context.LeaveApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                throw new InvalidOperationException("Leave application not found");

            if (application.Status != LeaveApplication.LeaveApplicationStatus.Pending)
                throw new InvalidOperationException("Only pending applications can be approved");

            var balance = await _context.EmployeeLeaveBalances
                .FirstAsync(b => b.EmployeeId == application.EmployeeId &&
                                 b.LeaveTypeId == application.LeaveTypeId);

            balance.TakenDays += application.DaysRequested;
            balance.AvailableDays -= application.DaysRequested;

            //GET MANAGER NAME
            var employee = await _context.Employees
                .FirstAsync(e => e.EmployeeId == application.EmployeeId);

            var manager = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employee.CareerManagerID);

            application.Status = LeaveApplication.LeaveApplicationStatus.Approved;
            application.DecisionDate = DateTime.UtcNow;
            application.DecisionBy = manager != null
                ? $"{manager.Name} {manager.Surname}"
                : "Admin";

            await _context.SaveChangesAsync();

            // ALWAYS SEND EMAIL
            await SendEmployeeDecisionEmail(application, true);
        }
        /// <summary>
        /// Rejects a pending leave application by validating the application ID and approval token,
        /// checking that the application is still pending and that the approval link has not expired,
        /// updating the application status to rejected, and sending an email notification to the employee about the rejection decision,
        /// while ensuring that all necessary validations are performed to maintain data integrity and that only authorized rejections are processed.
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="token"></param>
        /// <param name="reason"></param>
        public async Task RejectLeaveAsync(int applicationId, Guid token, string? reason)
        {
            var application = await _context.LeaveApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                throw new InvalidOperationException("Leave application not found");

            if (application.ApprovalToken != token)
                throw new InvalidOperationException("Invalid approval token");

            if (application.TokenExpiry < DateTime.UtcNow)
                throw new InvalidOperationException("Approval link expired");

            if (application.Status != LeaveApplication.LeaveApplicationStatus.Pending)
                throw new InvalidOperationException("Only pending applications can be rejected");

            application.Status = LeaveApplication.LeaveApplicationStatus.Rejected;
            application.DecisionDate = DateTime.UtcNow;
            var employee = await _context.Employees
            .FirstAsync(e => e.EmployeeId == application.EmployeeId);

            var manager = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employee.CareerManagerID);

            application.DecisionBy = manager != null
                ? $"{manager.Name} {manager.Surname}"
                : "Admin";
            application.RejectionReason = reason;

            await _context.SaveChangesAsync();

            await SendEmployeeDecisionEmail(application, false);
        }
        public async Task RejectLeaveInternalAsync(int applicationId, string? reason)
        {
            var application = await _context.LeaveApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                throw new InvalidOperationException("Leave application not found");

            if (application.Status != LeaveApplication.LeaveApplicationStatus.Pending)
                throw new InvalidOperationException("Only pending applications can be rejected");

            var employee = await _context.Employees
                .FirstAsync(e => e.EmployeeId == application.EmployeeId);

            var manager = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employee.CareerManagerID);

            application.Status = LeaveApplication.LeaveApplicationStatus.Rejected;
            application.DecisionDate = DateTime.UtcNow;
            application.DecisionBy = manager != null
                ? $"{manager.Name} {manager.Surname}"
                : "Admin";

            application.RejectionReason = reason;

            await _context.SaveChangesAsync();

            // 🔥 ALWAYS SEND EMAIL
            await SendEmployeeDecisionEmail(application, false);
        }
        /// <summary>
        /// Maps a LeaveApplication entity to a LeaveApplicationResponse DTO, 
        /// extracting relevant information such as employee ID, leave type ID, start and end dates, days requested, and application status,
        /// to provide a structured response object that can be returned to API clients while abstracting away internal entity details and ensuring that only necessary information is exposed.
        /// </summary>
        /// <param name="application"></param>
        private static LeaveApplicationResponse MapToResponse(LeaveApplication application)
        {
            return new LeaveApplicationResponse
            {
                Id = application.Id,
                EmployeeId = application.EmployeeId,
                LeaveTypeId = application.LeaveTypeId,
                StartDate = application.StartDate,
                EndDate = application.EndDate,
                DaysRequested = application.DaysRequested,
                Status = application.Status.ToString()
            };
        }
        /// <summary>
        /// Sends an email notification to the employee regarding the decision on their leave application,
        /// including details about the leave type, dates, and the decision (approved or rejected),
        /// </summary>
        /// <param name="application"></param>
        /// <param name="approved"></param>
        /// <returns></returns>
        private async Task SendEmployeeDecisionEmail(
    LeaveApplication application,
    bool approved)
        {
            var employee = await _context.Employees
                .FirstAsync(e => e.EmployeeId == application.EmployeeId);

            var leaveType = await _context.LeaveTypes
                .FirstAsync(l => l.Id == application.LeaveTypeId);

            var decision = approved ? "APPROVED" : "REJECTED";

            var emailBody = EmailTemplates.GenerateDecisionEmailHtml(
            employee,
            leaveType,
            application,
            approved
        );

            await _emailService.SendEmailAsync(
                employee.Email,
                "Leave Application Decision",
                emailBody
            );
        }
        /// <summary>
        /// Sends an email notification to the employee's manager requesting approval for a pending leave application,
        /// including details about the employee, leave type, requested dates, and links to approve or reject the application, 
        /// while ensuring that the email is sent to the correct manager based on the employee's career manager information and that the approval links contain secure tokens for validation.
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task SendManagerApprovalEmail(LeaveApplication application)
        {
            var employee = await _context.Employees
                .FirstAsync(e => e.EmployeeId == application.EmployeeId);

            var leaveType = await _context.LeaveTypes
                .FirstAsync(l => l.Id == application.LeaveTypeId);
            if (application.ApprovalToken == Guid.Empty)
            {
                application.ApprovalToken = Guid.NewGuid();
                application.TokenExpiry = DateTime.UtcNow.AddHours(48);
                await _context.SaveChangesAsync();
            }
            var baseUrl = _configuration["AppSettings:BaseUrl"];

            var approveLink =
                $"{baseUrl}/api/LeaveApplication/{application.Id}/approve?token={application.ApprovalToken}";

            var rejectLink =
                $"{baseUrl}/api/LeaveApplication/{application.Id}/reject?token={application.ApprovalToken}";

            var emailBody = EmailTemplates.GenerateApprovalEmailHtml(
            employee,
            leaveType,
            application,
            approveLink,
            rejectLink
        );

            var manager = await _context.Employees
     .FirstOrDefaultAsync(e => e.EmployeeId == employee.CareerManagerID);

            if (manager == null)
                throw new InvalidOperationException($"Manager not found for employee {employee.EmployeeId}");

            await _emailService.SendEmailAsync(
                manager.Email,
                "Leave Approval Required",
                emailBody
            );
        }

    }
}
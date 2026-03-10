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

        public LeaveApplicationService(
            ApplicationDBContext context,
            IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

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
            application.DecisionBy = "Manager";

            await _context.SaveChangesAsync();

            await SendEmployeeDecisionEmail(application, true);
        }

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
            application.DecisionBy = "Manager";
            application.RejectionReason = reason;

            await _context.SaveChangesAsync();

            await SendEmployeeDecisionEmail(application, false);
        }

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
        private async Task SendEmployeeDecisionEmail(
    LeaveApplication application,
    bool approved)
        {
            var employee = await _context.Employees
                .FirstAsync(e => e.EmployeeId == application.EmployeeId);

            var leaveType = await _context.LeaveTypes
                .FirstAsync(l => l.Id == application.LeaveTypeId);

            var decision = approved ? "APPROVED" : "REJECTED";

            var emailBody = $"""
<h2>Leave Application Update</h2>

<p>Hello {employee.FirstName},</p>

<p>Your leave request has been <strong>{decision}</strong>.</p>
{(approved ? "" : $"<p><strong>Reason:</strong> {application.RejectionReason}</p>")}

<p><strong>Leave Type:</strong> {leaveType.Name}</p>
<p><strong>Dates:</strong> {application.StartDate} to {application.EndDate}</p>
<p><strong>Days:</strong> {application.DaysRequested}</p>

<br/>

<p>Regards,<br/>HRConnect</p>
""";

            await _emailService.SendEmailAsync(
                employee.Email,
                "Leave Application Decision",
                emailBody
            );
        }
        private async Task SendManagerApprovalEmail(LeaveApplication application)
        {
            var employee = await _context.Employees
                .FirstAsync(e => e.EmployeeId == application.EmployeeId);

            var leaveType = await _context.LeaveTypes
                .FirstAsync(l => l.Id == application.LeaveTypeId);
            if (application.ApprovalToken == Guid.Empty)
            {
                application.ApprovalToken = Guid.NewGuid();
                application.TokenExpiry = DateTime.UtcNow.AddHours(48); // 48 hour approval window
                await _context.SaveChangesAsync();
            }
            var approveLink =
                $"http://localhost:5147/api/LeaveApplication/{application.Id}/approve?token={application.ApprovalToken}";

            var rejectLink =
                $"http://localhost:5147/api/LeaveApplication/{application.Id}/reject?token={application.ApprovalToken}";

            var emailBody = $"""
<h2>Leave Approval Required</h2>

<p><strong>Employee:</strong> {employee.FirstName} {employee.LastName}</p>
<p><strong>Leave Type:</strong> {leaveType.Name}</p>
<p><strong>Dates:</strong> {application.StartDate} to {application.EndDate}</p>
<p><strong>Days Requested:</strong> {application.DaysRequested}</p>

<br/>

<a href="{approveLink}" style="background-color:green;color:white;padding:10px;text-decoration:none;">
Approve Leave
</a>

&nbsp;&nbsp;

<a href="{rejectLink}" style="background-color:red;color:white;padding:10px;text-decoration:none;">
Reject Leave
</a>
""";

            await _emailService.SendEmailAsync(
                employee.ReportingManagerId, // manager email for now
                "Leave Approval Required",
                emailBody
            );
        }
        private string GenerateApprovalEmailHtml(
    Employee employee,
    LeaveType leaveType,
    LeaveApplication application,
    string approveLink,
    string rejectLink)
        {
            return $"""
<html>
<body style="font-family: Arial; background:#f4f6f8; padding:20px;">

<div style="max-width:600px;background:white;padding:30px;border-radius:8px">

<h2>Leave Approval Required</h2>

<p><strong>Employee:</strong> {employee.FirstName} {employee.LastName}</p>

<p><strong>Leave Type:</strong> {leaveType.Name}</p>

<p><strong>Dates:</strong> {application.StartDate} → {application.EndDate}</p>

<p><strong>Days Requested:</strong> {application.DaysRequested}</p>

<br>

<a href="{approveLink}"
style="
background:#2ecc71;
color:white;
padding:12px 25px;
text-decoration:none;
border-radius:6px;
margin-right:10px;
font-weight:bold;">
Approve
</a>

<a href="{rejectLink}"
style="
background:#e74c3c;
color:white;
padding:12px 25px;
text-decoration:none;
border-radius:6px;
font-weight:bold;">
Reject
</a>

<br><br>

<p style="color:#888;font-size:12px">
HRConnect Leave Management System
</p>

</div>

</body>
</html>
""";
        }
    }
}
namespace HRConnect.Api.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.Utils;
    using Microsoft.EntityFrameworkCore;
    using System.Globalization;

    public class LeaveApplicationService : ILeaveApplicationService
    {
        private readonly ApplicationDBContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ICloudinaryService _cloudinaryService;

        public LeaveApplicationService(
            ApplicationDBContext context,
            IEmailService emailService,
            IConfiguration configuration,
            ICloudinaryService cloudinaryService)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _cloudinaryService = cloudinaryService;
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

            if (leaveType.Code != "AL" &&
                (request.Documents == null || request.Documents.Count == 0))
            {
                throw new ArgumentException("Supporting documents are required.");
            }

            var allowedTypes = new[] { "image/png", "image/jpeg", "application/pdf" };
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".pdf" };
            const long maxFileSize = 5 * 1024 * 1024;

            var documents = new List<LeaveDocument>();
            var uploadedPublicIds = new List<string>();

            try
            {
                if (request.Documents != null)
                {
                    foreach (var file in request.Documents)
                    {
                        if (file.Length == 0)
                            throw new ArgumentException("Empty file is not allowed.");

                        if (file.Length > maxFileSize)
                            throw new ArgumentException("File exceeds 5MB.");

                        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                        if (!allowedTypes.Contains(file.ContentType) || !allowedExtensions.Contains(extension))
                            throw new ArgumentException("Invalid file type.");

                        var (url, publicId) = await _cloudinaryService.UploadFileAsync(file);

                        uploadedPublicIds.Add(publicId);

                        documents.Add(new LeaveDocument
                        {
                            FileName = file.FileName,
                            FileUrl = url,
                            PublicId = publicId,
                            FileType = file.ContentType,
                            FileSize = file.Length
                        });
                    }
                }
            }
            catch
            {
                //Rollback uploaded files
                foreach (var publicId in uploadedPublicIds)
                {
                    await _cloudinaryService.DeleteFileAsync(publicId);
                }

                throw;
            }

            var balance = employee.LeaveBalances
                .FirstOrDefault(lb => lb.LeaveTypeId == request.LeaveTypeId);

            if (balance == null)
                throw new InvalidOperationException("Leave balance not found.");

            var daysRequested =
                WorkingDayCalculator.CountWorkingDays(request.StartDate, request.EndDate);

            if (balance.AvailableDays <= 0 || balance.AvailableDays < daysRequested)
                throw new InvalidOperationException("Insufficient leave balance.");

            var application = new LeaveApplication
            {
                EmployeeId = request.EmployeeId,
                LeaveTypeId = request.LeaveTypeId,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DaysRequested = daysRequested,
                Documents = documents,
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
                .Include(a => a.Documents) // includes documents for email usage
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
                .FirstOrDefaultAsync(b => b.EmployeeId == application.EmployeeId &&
                                          b.LeaveTypeId == application.LeaveTypeId);

            if (balance == null)
                throw new InvalidOperationException("Leave balance not found");

            if (balance.AvailableDays < application.DaysRequested)
                throw new InvalidOperationException("Insufficient leave balance");

            balance.TakenDays += application.DaysRequested;
            balance.AvailableDays -= application.DaysRequested;

            application.Status = LeaveApplication.LeaveApplicationStatus.Approved;
            application.DecisionDate = DateTime.UtcNow;

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == application.EmployeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found");

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
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                throw new InvalidOperationException("Leave application not found");

            if (application.Status != LeaveApplication.LeaveApplicationStatus.Pending)
                throw new InvalidOperationException("Only pending applications can be approved");

            var balance = await _context.EmployeeLeaveBalances
                .FirstOrDefaultAsync(b => b.EmployeeId == application.EmployeeId &&
                                          b.LeaveTypeId == application.LeaveTypeId);

            if (balance == null)
                throw new InvalidOperationException("Leave balance not found");

            if (balance.AvailableDays < application.DaysRequested)
                throw new InvalidOperationException("Insufficient leave balance");

            balance.TakenDays += application.DaysRequested;
            balance.AvailableDays -= application.DaysRequested;

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == application.EmployeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            var manager = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employee.CareerManagerID);

            application.Status = LeaveApplication.LeaveApplicationStatus.Approved;
            application.DecisionDate = DateTime.UtcNow;
            application.DecisionBy = manager != null
                ? $"{manager.Name} {manager.Surname}"
                : "Admin";

            await _context.SaveChangesAsync();

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
                .Include(a => a.Documents) // ADDED: needed for email/document access
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                throw new InvalidOperationException("Leave application not found");

            if (application.ApprovalToken != token)
                throw new InvalidOperationException("Invalid approval token");

            if (application.TokenExpiry < DateTime.UtcNow)
                throw new InvalidOperationException("Approval link expired");

            if (application.Status != LeaveApplication.LeaveApplicationStatus.Pending)
                throw new InvalidOperationException("Only pending applications can be rejected");

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == application.EmployeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            var manager = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employee.CareerManagerID);

            application.Status = LeaveApplication.LeaveApplicationStatus.Rejected;
            application.DecisionDate = DateTime.UtcNow;
            application.DecisionBy = manager != null
                ? $"{manager.Name} {manager.Surname}"
                : "Admin";

            application.RejectionReason = string.IsNullOrWhiteSpace(reason)
                ? "No reason provided" // fallback if no reason is given
                : reason;

            await _context.SaveChangesAsync();

            await SendEmployeeDecisionEmail(application, false);
        }
        public async Task RejectLeaveInternalAsync(int applicationId, string? reason)
        {
            var application = await _context.LeaveApplications
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                throw new InvalidOperationException("Leave application not found");

            if (application.Status != LeaveApplication.LeaveApplicationStatus.Pending)
                throw new InvalidOperationException("Only pending applications can be rejected");

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == application.EmployeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            var manager = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employee.CareerManagerID);

            application.Status = LeaveApplication.LeaveApplicationStatus.Rejected;
            application.DecisionDate = DateTime.UtcNow;
            application.DecisionBy = manager != null
                ? $"{manager.Name} {manager.Surname}"
                : "Admin";

            application.RejectionReason = string.IsNullOrWhiteSpace(reason)
                ? "No reason provided"
                : reason;

            await _context.SaveChangesAsync();

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
                EmployeeName = application.Employee != null
                    ? $"{application.Employee.Name} {application.Employee.Surname}"
                    : "Unknown",

                LeaveTypeId = application.LeaveTypeId,
                LeaveTypeCode = application.LeaveType?.Code ?? string.Empty,

                StartDate = application.StartDate,
                EndDate = application.EndDate,
                DaysRequested = application.DaysRequested,
                DaysAllocated = application.LeaveType?.EntitlementRules != null &&
                                application.LeaveType.EntitlementRules.Count > 0
                    ? application.LeaveType.EntitlementRules
                        .Where(r => r.IsActive)
                        .OrderByDescending(r => r.MinYearsService)
                        .Select(r => r.DaysAllocated)
                        .FirstOrDefault()
                    : 0,

                Status = application.Status.ToString(),

                Documents = application.Documents?.Select(d => new LeaveDocumentResponse
                {
                    FileName = d.FileName,
                    FileUrl = d.FileUrl
                }).ToList() ?? new List<LeaveDocumentResponse>()
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
                .FirstOrDefaultAsync(e => e.EmployeeId == application.EmployeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(l => l.Id == application.LeaveTypeId);

            if (leaveType == null)
                throw new InvalidOperationException("Leave type not found");

            var decision = approved ? "APPROVED" : "REJECTED";

            var emailBody = EmailTemplates.GenerateDecisionEmailHtml(
                employee,
                leaveType,
                application,
                approved
            );

            var documentLinks = "";

            if (application.Documents != null && application.Documents.Count > 0)
            {
                documentLinks = "<br/><br/><strong>Supporting Documents:</strong><br/>" +
                    string.Join("<br/>",
                        application.Documents.Select(d =>
                            $"<a href='{System.Net.WebUtility.HtmlEncode(d.FileUrl)}' target='_blank'>" +
                            $"{System.Net.WebUtility.HtmlEncode(d.FileName)}</a>"));
            }

            emailBody += documentLinks;

            await _emailService.SendEmailAsync(
                employee.Email,
                $"Leave Application {decision}",
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
                .FirstOrDefaultAsync(e => e.EmployeeId == application.EmployeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(l => l.Id == application.LeaveTypeId);

            if (leaveType == null)
                throw new InvalidOperationException("Leave type not found");

            if (application.ApprovalToken == Guid.Empty)
            {
                application.ApprovalToken = Guid.NewGuid();
                application.TokenExpiry = DateTime.UtcNow.AddHours(48);
            }

            var baseUrl = _configuration["AppSettings:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("Base URL is not configured");

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
        public async Task<List<LeaveApplicationResponse>> GetAllAsync()
        {
            var applications = await _context.LeaveApplications
                .AsNoTracking()
                .Include(a => a.Employee)
                .Include(a => a.LeaveType)
                    .ThenInclude(lt => lt.EntitlementRules)
                .Include(a => a.Documents)
                .ToListAsync();

            if (applications == null || applications.Count == 0)
                return new List<LeaveApplicationResponse>();

            return applications.Select(a => new LeaveApplicationResponse
            {
                Id = a.Id,

                EmployeeName = a.Employee != null
                    ? $"{a.Employee.Name} {a.Employee.Surname}"
                    : "Unknown",

                LeaveTypeId = a.LeaveTypeId,

                LeaveTypeCode = a.LeaveType?.Code ?? string.Empty,

                StartDate = a.StartDate,
                EndDate = a.EndDate,
                DaysRequested = a.DaysRequested,

                DaysAllocated = a.LeaveType?.EntitlementRules != null && a.LeaveType.EntitlementRules.Count > 0
                    ? a.LeaveType.EntitlementRules
                        .Where(r => r.IsActive)
                        .OrderByDescending(r => r.MinYearsService)
                        .Select(r => r.DaysAllocated)
                        .FirstOrDefault()
                    : 0, // fallback

                Status = a.Status.ToString(),

                Documents = a.Documents != null && a.Documents.Count > 0
                    ? a.Documents.Select(d => new LeaveDocumentResponse
                    {
                        FileName = d.FileName,
                        FileUrl = d.FileUrl
                    }).ToList()
                    : new List<LeaveDocumentResponse>()
            }).ToList();
        }
        public async Task<List<LeaveApplicationResponse>> GetByLeaveTypeCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Leave type code is required.");

            code = code.Trim().ToUpperInvariant();

            var applications = await _context.LeaveApplications
                .AsNoTracking()
                .Include(a => a.Employee)
                .Include(a => a.LeaveType)
                    .ThenInclude(lt => lt.EntitlementRules)
                .Include(a => a.Documents)
                .Where(a => a.LeaveType.Code == code)
                .ToListAsync();

            if (applications == null || applications.Count == 0)
                return new List<LeaveApplicationResponse>();

            return applications.Select(a => new LeaveApplicationResponse
            {
                Id = a.Id,

                EmployeeName = a.Employee != null
                    ? $"{a.Employee.Name} {a.Employee.Surname}"
                    : "Unknown",

                LeaveTypeId = a.LeaveTypeId,

                LeaveTypeCode = a.LeaveType?.Code ?? string.Empty,

                StartDate = a.StartDate,
                EndDate = a.EndDate,
                DaysRequested = a.DaysRequested,

                DaysAllocated = a.LeaveType?.EntitlementRules != null && a.LeaveType.EntitlementRules.Count > 0
                    ? a.LeaveType.EntitlementRules
                        .Where(r => r.IsActive)
                        .OrderByDescending(r => r.MinYearsService)
                        .Select(r => r.DaysAllocated)
                        .FirstOrDefault()
                    : 0,

                Status = a.Status.ToString(),

                Documents = a.Documents != null && a.Documents.Count > 0
                    ? a.Documents.Select(d => new LeaveDocumentResponse
                    {
                        FileName = d.FileName,
                        FileUrl = d.FileUrl
                    }).ToList()
                    : new List<LeaveDocumentResponse>()
            }).ToList();
        }
        public async Task<List<LeaveApplicationResponse>> GetByStatusAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status is required.");

            status = status.Trim();

            if (!Enum.TryParse<LeaveApplication.LeaveApplicationStatus>(
                status, true, out var parsedStatus))
            {
                throw new ArgumentException("Invalid status value. Use Pending, Approved, or Rejected.");
            }

            var applications = await _context.LeaveApplications
                .AsNoTracking()
                .Include(a => a.Employee)
                .Include(a => a.LeaveType)
                    .ThenInclude(lt => lt.EntitlementRules)
                .Include(a => a.Documents)
                .Where(a => a.Status == parsedStatus)
                .ToListAsync();

            if (applications == null || applications.Count == 0)
                return new List<LeaveApplicationResponse>();

            return applications.Select(a => new LeaveApplicationResponse
            {
                Id = a.Id,

                EmployeeName = a.Employee != null
                    ? $"{a.Employee.Name} {a.Employee.Surname}"
                    : "Unknown",

                LeaveTypeId = a.LeaveTypeId,

                LeaveTypeCode = a.LeaveType?.Code ?? string.Empty,

                StartDate = a.StartDate,
                EndDate = a.EndDate,
                DaysRequested = a.DaysRequested,

                DaysAllocated = a.LeaveType?.EntitlementRules != null && a.LeaveType.EntitlementRules.Count > 0
                    ? a.LeaveType.EntitlementRules
                        .Where(r => r.IsActive)
                        .OrderByDescending(r => r.MinYearsService)
                        .Select(r => r.DaysAllocated)
                        .FirstOrDefault()
                    : 0,

                Status = a.Status.ToString(),

                Documents = a.Documents != null && a.Documents.Count > 0
                    ? a.Documents.Select(d => new LeaveDocumentResponse
                    {
                        FileName = d.FileName,
                        FileUrl = d.FileUrl
                    }).ToList()
                    : new List<LeaveDocumentResponse>()
            }).ToList();
        }

    }
}
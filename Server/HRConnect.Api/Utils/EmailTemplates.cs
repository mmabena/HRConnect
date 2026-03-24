namespace HRConnect.Api.Utils
{
    using HRConnect.Api.Models;

    public static class EmailTemplates
    {
        /// <summary>
        /// Generates the HTML content for the leave approval email, 
        /// including employee details, leave type, dates, and action links for approval or rejection.
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="leaveType"></param>
        /// <param name="application"></param>
        /// <param name="approveLink"></param>
        /// <param name="rejectLink"></param>
        /// <returns></returns>
        public static string GenerateApprovalEmailHtml(
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

                    <p><strong>Employee:</strong> {employee.Name} {employee.Surname}</p>
                    <p><strong>Leave Type:</strong> {leaveType.Name}</p>
                    <p><strong>Dates:</strong> {application.StartDate} → {application.EndDate}</p>
                    <p><strong>Days Requested:</strong> {application.DaysRequested}</p>

                    <br>

                    <a href="{approveLink}"
                    style="background:#2ecc71;color:white;padding:12px 25px;text-decoration:none;border-radius:6px;margin-right:10px;font-weight:bold;">
                    Approve
                    </a>

                    <a href="{rejectLink}"
                    style="background:#e74c3c;color:white;padding:12px 25px;text-decoration:none;border-radius:6px;font-weight:bold;">
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
        /// <summary>
        /// Generates the HTML content for the leave decision email, 
        /// informing the employee about the approval or rejection of their leave request, 
        /// including details of the leave and any rejection reason if applicable.
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="leaveType"></param>
        /// <param name="application"></param>
        /// <param name="approved"></param>
        /// <returns></returns>
        public static string GenerateDecisionEmailHtml(
            Employee employee,
            LeaveType leaveType,
            LeaveApplication application,
            bool approved)
        {
            var decision = approved ? "APPROVED" : "REJECTED";

            return $"""
                    <h2>Leave Application Update</h2>

                    <p>Hello {employee.Name},</p>

                    <p>Your leave request has been <strong>{decision}</strong>.</p>
                    {(approved ? "" : $"<p><strong>Reason:</strong> {application.RejectionReason}</p>")}

                    <p><strong>Leave Type:</strong> {leaveType.Name}</p>
                    <p><strong>Dates:</strong> {application.StartDate} to {application.EndDate}</p>
                    <p><strong>Days:</strong> {application.DaysRequested}</p>

                    <br/>

                    <p>Regards,<br/>HRConnect</p>
                    """;
        }
        /// <summary>
        /// Generates the HTML content for the position update email, 
        /// notifying the employee about a change in their position and the resulting recalculation of their annual leave entitlement, 
        /// including the new entitlement, used days, and available days.
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="accruedDays"></param>
        /// <param name="takenDays"></param>
        /// <param name="availableDays"></param>
        /// <returns></returns>
        public static string GeneratePositionUpdateEmail(
    Employee employee,
    decimal accruedDays,
    decimal takenDays,
    decimal availableDays)
        {
            return $"""
                    Dear {employee.Name},
                    
                    Your position has recently been updated to: {employee.Position.PositionTitle}.
                    
                    As a result, your annual leave entitlement has been recalculated.
                    
                    New Annual Entitlement: {accruedDays} days
                    Used Days: {takenDays} days
                    Available Days: {availableDays} days
                    
                    This adjustment was calculated proportionally based on the promotion date.
                    
                    If you have any questions, please contact HR.
                    
                    Regards,
                    HRConnect
                    """;
        }
        /// <summary>
        /// Generates the HTML content for the carryover warning email, 
        /// informing the employee about their remaining annual leave balance, 
        /// the carryover policy, and any days that will be forfeited if not used before the end of the year.
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="availableDays"></param>
        /// <param name="forfeitedDays"></param>
        /// <returns></returns>
        public static string GenerateCarryOverWarningEmail(
    Employee employee,
    decimal availableDays,
    decimal forfeitedDays)
        {
            return $"""
                    Dear {employee.Name},

                    You currently have {availableDays} days of Annual Leave remaining.

                    Only 5 days can be carried over into the next year.
                    {forfeitedDays} days will be forfeited if its not used before 31 December.

                    Regards,
                    HRConnect
                    """;
        }
        /// <summary>
        /// Generates the HTML content for the leave rule change email, 
        /// notifying the employee about a change in the leave entitlement rule that affects them, 
        /// including the new entitlement and available days.
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="newEntitlement"></param>
        /// <param name="availableDays"></param>
        /// <returns></returns>
        public static string GenerateRuleChangeEmail(
    Employee employee,
    decimal newEntitlement,
    decimal availableDays)
        {
            return $"""
                    Dear {employee.Name},
                    
                    The company has updated the leave policy.
                    
                    Your new annual entitlement is {newEntitlement} days.
                    Your available balance is now {availableDays} days.
                    
                    Regards,
                    HRConnect
                    """;
        }
    }
}
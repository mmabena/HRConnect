namespace HRConnect.Api.DTOs.Notification
{
    using HRConnect.Api.Models;
    public class CreateNotificationDto
    {
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; } // Leave, Tax, Payroll
        public NotificationSeverity Severity { get; set; }
        //This will also determine if a user is a superuer
        public string EmployeeId { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DeliveryChannel DeliveryChannel { get; set; } //"Email", "InApp" etc       
    }
}
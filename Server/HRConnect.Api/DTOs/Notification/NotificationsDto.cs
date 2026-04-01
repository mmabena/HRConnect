namespace HRConnect.Api.DTOs.Notification
{
    using HRConnect.Api.Models;
    public class NotificationDto
    {
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public string Type { get; set; } = string.Empty;
        public NotificationSeverity Severity { get; set; }
        public DateTime DueDate { get; set; }
        // public DateTime? ExpirationDate { get; set; }
        // public DateTime CreatedAt { get; set; }
    }
}
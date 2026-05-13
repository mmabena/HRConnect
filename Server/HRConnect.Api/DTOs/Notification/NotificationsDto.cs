namespace HRConnect.Api.DTOs.Notification
{
  using HRConnect.Api.Models;
  public class NotificationDto
  {
    public string Message { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public NotificationType Type { get; set; }
    public NotificationSeverity Severity { get; set; }
    public DateTime? DueDate { get; set; }
    public DeliveryChannel DeliveryChannel { get; set; }
    public DateTime CreatedAt { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
  }
}
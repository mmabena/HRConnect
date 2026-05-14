namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  public enum NotificationSeverity
  {
    Critical,
    Warning,
    Infomation
  }
  public enum NotificationType
  {
    Payroll, TaxUpload, RoleUpdate, LeaveRequest, LeaveRequestResponse, General
  }
  [Flags]
  public enum DeliveryChannel
  {
    InApp = 0, Email
  };

  public class Notification
  {
    [Key]
    public int NotificationId { get; set; }
    public string IdempotencyKey { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public NotificationType Type { get; set; } // Leave, Tax, Payroll
    public NotificationSeverity Severity { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DeliveryChannel DeliveryChannel { get; set; } //Email", "InApp" etc
  }
}
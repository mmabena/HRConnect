namespace HRConnect.Api.Models
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  public enum NotificationSeverity
  {
    Critial,
    Warning,
    Infomation
  }
  public class Notification
  {
    [Key]
    public int NotificationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string Type { get; set; } = string.Empty;
    public NotificationSeverity Severity { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
  }
}
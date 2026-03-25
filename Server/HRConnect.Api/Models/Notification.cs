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
    Payroll, TaxUpload, LeaveRequest, LeaveRequestResponse, General
  }

  public class Notification
  {
    [Key]
    public int NotificationId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public NotificationType Type { get; set; } // Leave, Tax, Payroll
    public NotificationSeverity Severity { get; set; }
    //This will also determine if a user is a superuer
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }//Event-sensitive notifications rely 
                                          // on this date for persistence
                                          //Idk how to use this right now

    // public DateTime? ScheduledFor { get; set; }
    // public DateTime? ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string DeliveryChannel { get; set; } = string.Empty;//"Email", "InApp" etc
  }
}
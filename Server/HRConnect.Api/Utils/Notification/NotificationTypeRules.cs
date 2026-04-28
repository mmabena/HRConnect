namespace HRConnect.Api.Utils.Notification
{
  using HRConnect.Api.Models;

  public static class NotificationTypeRules
  {
    public static bool ShouldPersist(NotificationSeverity severity)
    {
      //using pattern matching instead of boolean checks
      return severity is NotificationSeverity.Critical;
    }
    public static bool RequiresAction(NotificationType type)
    {
      return type is NotificationType.TaxUpload;
    }
    public static bool IsHighestPriority(NotificationType type)
    {
      return type is NotificationType.Payroll or
    NotificationType.TaxUpload;
    }
  }
}
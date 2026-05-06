namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Notification;
  public interface INotificationService
  {
    Task MarkNotificationReadByTypeAsync(NotificationType type, string employeeId);
    Task CreateAndDispatchAsync(Notification notification);
    Task CreateOrEnsureExistsAsync(Notification notification);
    Task MarkBatchedNotificationsReadByTypeAsync(NotificationType type, List<string> employeeIds);
    Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsBySeverityAsync(NotificationSeverity severity, string employeeId);
    Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsByTypeAsync(NotificationType type, string employeeId);
    Task TryCreateAndDispatch(Notification notification);
    Task<bool> DeleteAllReadAsync();
    Task<bool> DeleteAllByEmployeeIdAsync(string employeeId);
  }
}
namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.DTOs.Notification;
  using HRConnect.Api.Models;

  public interface INotificationService
  {
    Task MarkNotificationReadByTypeAsync(NotificationType type, int userId);
    Task<IEnumerable<NotificationDto>> GetEmployeeNotificationsAsync(int userId);
    Task MarkBatchedNotificationsReadByTypeAsync(NotificationType type, List<string> employeeIds);
    Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsBySeverityAsync(
      NotificationSeverity severity,
      int userId
    );
    Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsByTypeAsync(
      NotificationType type,
      int userId
    );

    Task TryCreateAndDispatch(Notification notification);
    Task<bool> DeleteAllReadAsync();
    Task<bool> DeleteAllByEmployeeIdAsync(int userId);
    Task<bool> DeleteNotificationByIdAsync(int userId, int id);
  }
}

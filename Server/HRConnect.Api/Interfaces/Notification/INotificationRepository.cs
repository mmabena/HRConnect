namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Notification;
  public interface INotificationRepository
  {
    Task<Notification> AddNotificationAsync(Notification notification);
    Task<Notification?> ExistsAsync(NotificationType type, string employeeId, string? message, NotificationSeverity severity);
    Task MarkBatchAsReadAsync(List<string> employeeIds, NotificationType type);
    Task MarkAsReadAsync(Notification notification);
    Task<bool> Save();
    Task<Notification> TryAndAquireAsync(string idempotencyKey);
    Task<IEnumerable<NotificationDto>> GetAllUnreadAsync(string? employeeId);
    Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsBySeverityAsync(string employeeId, NotificationSeverity severity);
    Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsByTypeAsync(NotificationType type, string employeeId);
    Task<Notification?> TryCreateUnreadAsync(Notification notification);
  }
}
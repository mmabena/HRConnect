namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Notification;
  internal interface INotificationRepository
  {
    Task AddNotificationAsync(Notification notification);
    Task<Notification?> ExistsAsync(NotificationType type, string employeeId, string? message, NotificationSeverity severity);
    Task<bool> MarkAsReadAsync(Notification notification);
    Task Save();
    Task<bool> TryAndAquireAsync(string idempotencyKey, string deliveryChannel);
    Task<IEnumerable<NotificationDto>> GetAllUnreadAsync(string? employeeId);
    Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsBySeverityAsync(string employeeId, NotificationSeverity severity);
    Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsByTypeAsync(NotificationType type, string employeeId);
  }
}
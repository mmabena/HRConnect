namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Notification;
  public interface INotificationService
  {
    Task CreateAndDispatchAsync(Notification notification);
    Task CreateOrEnsureExistsAsync(Notification notification);
    Task<IList<NotificationDto>> GetAllEmployeeNotificationsByTypeAsync(NotificationType type, string employeeId);
    //Critical,Warning,Information
    Task<IList<NotificationDto>> GetAllEmployeeNotificationsBySeverityAsync(string employeeId,
    NotificationSeverity severity);

  }
}
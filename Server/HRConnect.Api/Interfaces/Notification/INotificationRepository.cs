namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Notification;
  public interface INotificationRepository
  {
    // Task <NotificationDto> CreateNotification
    Task AddNotificationAsync(Notification notification);
    Task<Notification?> ExistsAsync(NotificationType type, string employeeId);
    Task<bool> MarkAsReadAsync(Notification notification);
    //May not be necessary for production
    Task<IEnumerable<NotificationDto>> GetAllUnreadAsync(string? employeeId);
  }
}
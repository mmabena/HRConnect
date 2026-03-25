namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Notification;
  public interface INotificationRepository
  {
    // Task <NotificationDto> CreateNotification
    Task AddNotificationAsync(Notification notification);
    Task<Notification?> ExistsAsync(NotificationType type, DateTime? dueDate, DateTime dateTime);
    Task<bool> MarkAsReadAsync(int id);
    //May not be necessary for production
    Task<IEnumerable<NotificationDto>> GetAllUnreadAsync(string? employeeId);
  }
}
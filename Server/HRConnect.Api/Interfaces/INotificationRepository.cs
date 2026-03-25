namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Notification;
  public interface INotificationRepository
  {
    // Task <NotificationDto> CreateNotification
    Task AddNotificationAsync(Notification notification);
    Task<bool> ExistsAsync(string type, DateTime executionDate, DateTime dateTime);
    Task<bool> MarkAsReadAsync(int id);
    Task<IEnumerable<NotificationDto>> GetAllUnreadAsync();
  }
}
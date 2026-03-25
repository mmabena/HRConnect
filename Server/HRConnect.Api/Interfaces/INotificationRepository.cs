namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;

  public interface INotificationRepository
  {
    Task AddNotification(Notification noti);
    Task<bool> ExistsAsync(string type, DateTime dateTime);
  }
}
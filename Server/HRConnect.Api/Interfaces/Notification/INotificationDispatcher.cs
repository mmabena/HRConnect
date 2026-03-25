namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.Models;
  public interface INotificationDispatcher
  {
    Task DispatchNotificationAsync(Notification notification);
  }
}
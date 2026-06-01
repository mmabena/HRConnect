namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.Models;
  /// <summary>
  /// Interface to dispatch notification through to the correct the delivery channel
  /// </summary>
  public interface INotificationDispatcher
  {
    Task DispatchNotificationAsync(Notification notification);
  }
}
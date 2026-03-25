namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.Models;
  public interface INotificationDeliveryChannel
  {
    string Name { get; }
    Task SendAsync(Notification notification);
  }
}
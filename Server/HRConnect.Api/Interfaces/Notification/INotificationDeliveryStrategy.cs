namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.Models;
  public interface INotificationDeliveryStrategy
  {
    string Name { get; }
    Task SendNotificationAsync(Notification notification);
  }
}
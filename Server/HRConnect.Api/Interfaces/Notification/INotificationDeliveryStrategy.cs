namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.Models;
  public interface INotificationDeliveryChannel
  {
    string Name { get; }
    DeliveryChannel Channel { get; }
    Task SendNotificationAsync(Notification notification);
  }
}
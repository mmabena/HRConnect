namespace HRConnect.Api.Utils.Notification.Channels
{
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Models;
  public class InAppDeliveryStrategy : INotificationDeliveryChannel
  {
    public string Name => "In-App";
    public DeliveryChannel Channel => DeliveryChannel.InApp;
    public async Task SendNotificationAsync(Notification notification)
    {
      //In App notifications are sent via api.
      //No Action is required, frontend should fetch the created notification 
      //from the database
      await Task.CompletedTask;
    }
  }
}
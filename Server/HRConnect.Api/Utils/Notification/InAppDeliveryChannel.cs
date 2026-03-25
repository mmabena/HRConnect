namespace HRConnect.Api.Utils.Notification
{
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Models;
  public class InAppDeliveryChannel : INotificationDeliveryChannel
  {
    public string Name => "In-App";
    public async Task SendAsync(Notification notification)
    {
      //In App notifications are sent via api.
      //No Action is required, frontend should fetch the created notification 
      //from the database
      await Task.CompletedTask;
    }
  }
}
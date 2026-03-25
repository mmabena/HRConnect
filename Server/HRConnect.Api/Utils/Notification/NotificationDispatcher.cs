namespace HRConnect.Api.Utils.Notification
{
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Models;

  public class NotificationDispatcher : INotificationDispatcher
  {
    private readonly IEnumerable<INotificationDeliveryChannel> _deliveryChannels;
    public NotificationDispatcher(IEnumerable<INotificationDeliveryChannel> deliveryChannels)
    {
      _deliveryChannels = deliveryChannels;
    }
    public async Task DispatchNotificationAsync(Notification notification)
    {
      foreach (var channel in _deliveryChannels)
      {
        try
        {
          await channel.SendAsync(notification);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Failed To Send Notifaction Through Channel {channel.Name}");
          Console.WriteLine($"{ex.Message}");
        }
      }
    }
  }
}

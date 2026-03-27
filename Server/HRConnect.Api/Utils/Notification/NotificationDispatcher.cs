namespace HRConnect.Api.Utils.Notification
{
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Models;

  public class NotificationDispatcher : INotificationDispatcher
  {
    private readonly IEnumerable<INotificationDeliveryStrategy> _deliveryStrategies;
    public NotificationDispatcher(IEnumerable<INotificationDeliveryStrategy> deliveryStrategies)
    {
      _deliveryStrategies = deliveryStrategies;
    }
    public async Task DispatchNotificationAsync(Notification notification)
    {
      foreach (var strategy in _deliveryStrategies)
      {
        try
        {
          await strategy.SendNotificationAsync(notification);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Failed To Send Notifaction Through Channel {strategy.Name}");
          Console.WriteLine($"{ex.Message}");
        }
      }
    }
  }
}
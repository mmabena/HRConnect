namespace HRConnect.Api.Utils.Notification
{
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Models;

  public class NotificationDispatcher : INotificationDispatcher
  {
    private readonly IEnumerable<INotificationDeliveryChannel> _deliveryStrategies;
    public NotificationDispatcher(IEnumerable<INotificationDeliveryChannel> deliveryStrategies)
    {
      _deliveryStrategies = deliveryStrategies;
    }
    public async Task DispatchNotificationAsync(Notification notification)
    {

      foreach (var strategy in ResolveDeliveryStrategy(notification.DeliveryChannel))
      {
        try
        {
          await strategy.SendNotificationAsync(notification);
        }
        catch (InvalidOperationException ex)
        {
          throw new InvalidDataException($"Failed To Send/Dispatch Through Channel {ex?.Message}");
        }
      }
    }

    /// <summary>
    /// Resolves injected dependencies to only enumerate through delivery channels that are needed for the batch of notifications going out 
    /// </summary>
    /// <param name="deliveryChannels">Enum annotated with [Flags] of delivery channels </param>
    /// <returns>Resolved Channels that have notifications in the pipeline for them</returns>
    private IEnumerable<INotificationDeliveryChannel> ResolveDeliveryStrategy(DeliveryChannel deliveryChannels)
    {
      return _deliveryStrategies.Where(s => (deliveryChannels & s.Channel) == s.Channel);
    }
  }
}
namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.Models;
  /// <summary>
  /// Interface that needs to be implemented by any Notification Delivery Channel  
  /// </summary>
  public interface INotificationDeliveryChannel
  {
    string Name { get; }
    DeliveryChannel Channel { get; }
    /// <summary>
    /// Overridable function that each class that implements this interface has to implement
    /// </summary>
    /// <param name="notification"></param>
    /// <returns></returns>
    Task SendNotificationAsync(Notification notification);
  }
}
namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.Models;
  /// <summary>
  /// Producer interface to be able to create a notifications
  ///This is a event-Based Design instead of just CRUD-Designs 
  /// </summary>
  /// <remarks>Producers should not worry about how notifications would be 
  /// delivered. This will be handled by <see cref="NotificationDeliverer"/>
  /// </remarks>
  public interface INotificationFactory
  {
    /// <summary>
    /// Produces and publishes the notifications to while preventing duplicating notifications
    /// This method should be called based on the event that triggers or 'produces' a notification
    /// </summary>
    /// <param name="notification">Notification to publish to the database</param>
    Task ProduceNotificationAsync(Notification notification);
  }
}
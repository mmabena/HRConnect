namespace HRConnect.Api.Utils.Factories
{
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.DTOs.Notification;
  using HRConnect.Api.Mappers.Notification;
  using HRConnect.Api.Models;

  /// <summary>
  ///  Classs is used to create a notifications using the Factory Design Pattern.
  ///  It is injected/called when one wants to create a notifcation 
  ///  All Notifications passed to the factory are dispatched/sent through to the correct 
  ///  delivery channels (using the Strategy Pattern)
  /// </summary>
  public class NotificationFactory : INotificationFactory
  {
    private readonly INotificationService _notificationService;
    public NotificationFactory(INotificationService notificationService)
    {
      _notificationService = notificationService;
    }
    /// <summary>
    /// Preferred starting point for creating a notification. 
    /// This is the only function to create a notification within the system
    /// Email notifications are marked as Read before saving to database.
    /// The Idempotency key of all notification depend on the IsRead status of 
    /// all incoming notifications
    /// </summary>
    /// <param name="notification">The DTO used to create a notification.</param>
    ///<remarks> Some members of the notification are abstracted away from the developer as 
    /// these are automatically initialised.
    ///</remarks>
    public async Task ProduceNotificationAsync(CreateNotificationDto notification)
    {
      Notification newNotification = notification.ToNotificationFromCreateDto();
      newNotification.CreatedAt = DateTime.Now;
      if (notification.DeliveryChannel == DeliveryChannel.Email)
      {
        newNotification.IsRead = true;
      }
      else
      {
        newNotification.IsRead = false;
      }
      await _notificationService.TryCreateAndDispatch(newNotification);
    }
  }
}
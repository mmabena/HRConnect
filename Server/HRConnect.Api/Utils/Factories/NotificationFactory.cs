namespace HRConnect.Api.Utils.Factories
{
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.DTOs.Notification;
  using HRConnect.Api.Mappers.Notification;

  public class NotificationFactory : INotificationFactory
  {
    private readonly INotificationService _notificationService;
    public NotificationFactory(INotificationService notificationService)
    {
      _notificationService = notificationService;
    }
    public async Task ProduceNotificationAsync(CreateNotificationDto notification)
    {
      var newNoti = notification.ToNotificationFromDto();
      newNoti.CreatedAt = DateTime.Now;
      newNoti.IsRead = false;
      // await _notificationService.CreateOrEnsureExistsAsync(newNoti);
      await _notificationService.CreateAndDispatchAsync(newNoti);
    }
  }
}
namespace HRConnect.Api.Utils.Factories
{
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.DTOs.Notification;
  using HRConnect.Api.Mappers.Notification;
  using HRConnect.Api.Models;

  public class NotificationFactory : INotificationFactory
  {
    private readonly INotificationService _notificationService;
    public NotificationFactory(INotificationService notificationService)
    {
      _notificationService = notificationService;
    }
    public async Task ProduceNotificationAsync(CreateNotificationDto notification)
    {
      Notification newNoti = notification.ToNotificationFromCreateDto();
      newNoti.CreatedAt = DateTime.Now;
      newNoti.IsRead = false;
      await _notificationService.TryCreateAndDispatch(newNoti);
    }
  }
}
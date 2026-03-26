namespace HRConnect.Api.Utils.Factories
{
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Services;
  using HRConnect.Api.Models;
  public class NotificationFactory : INotificationFactory
  {
    private readonly INotificationService _notificationService;
    public NotificationFactory(INotificationService notificationService)
    {
      _notificationService = notificationService;
    }
    public async Task ProduceNotificationAsync(Notification notification)
    {
      throw new NotImplementedException();
    }
  }
}
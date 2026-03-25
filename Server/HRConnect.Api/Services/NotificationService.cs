namespace HRConnect.Api.Services
{
  using HRConnect.Api.Models;
  using System.Threading.Tasks;
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Utils.Notification;
  public class NotificationService : INotificationService
  {
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationDispatcher _notificationDispatcher;
    public NotificationService(INotificationRepository notificationRepository, INotificationDispatcher notificationDispatcher)
    {
      _notificationRepository = notificationRepository;
      _notificationDispatcher = notificationDispatcher;
    }
    public async Task CreateAndDispatchAsync(Notification notification)
    {
      //Ensure that we don't have duplicate notifications before storing
      //Alternatively filter in a range of DueDates and CreatedDates
      var notificationExists = await _notificationRepository.ExistsAsync(notification.Type, notification.DueDate, notification.CreatedAt);

      if (notificationExists == null)
      {
        return;
      }

      //create and store the notification
      notification.CreatedAt = DateTime.Now;
      await _notificationRepository.AddNotificationAsync(notification);
    }

    public async Task CreateOrEnsureExistsAsync(Notification notification)
    {
      bool isPesistent = NotificationTypeRules.ShouldPersist(notification.Type);
      if (isPesistent)
      {
        //Find if it already exists 
        var existing = await _notificationRepository.ExistsAsync(notification.Type, notification.DueDate, notification.CreatedAt);

        if (existing != null)
        {
          //Keep the Payroll and Tax Table uploads notifications persistent
          notification.IsRead = false;
          return;
        }

        //For other general notifications
      }
    }
  }
}
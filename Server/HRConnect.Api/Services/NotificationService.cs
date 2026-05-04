namespace HRConnect.Api.Services
{
  using HRConnect.Api.Models;
  using System.Threading.Tasks;
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.DTOs.Notification;
  using HRConnect.Api.Utils.Notification;
  using System.Text;
  using System.Security.Cryptography;
  using HRConnect.Api.Mappers.Notification;

  public class NotificationService : INotificationService
  {
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationDispatcher _notificationDispatcher;
    public NotificationService(INotificationRepository notificationRepository, INotificationDispatcher notificationDispatcher)
    {
      _notificationRepository = notificationRepository;
      _notificationDispatcher = notificationDispatcher;
    }
    public async Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsByTypeAsync(NotificationType type, string employeeId)
    {
      IEnumerable<NotificationDto> notifications = await _notificationRepository.GetAllEmployeeNotificationsByTypeAsync(type, employeeId);

      return notifications;
    }
    public async Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsBySeverityAsync(NotificationSeverity severity, string employeeId)
    {
      IEnumerable<NotificationDto> notifications = await _notificationRepository.GetAllEmployeeNotificationsBySeverityAsync(employeeId, severity);

      return notifications;
    }
    public async Task MarkBatchedNotificationsReadByTypeAsync(NotificationType type, List<string> employeeIds)
    {
      await _notificationRepository.MarkBatchAsReadAsync(employeeIds, type);
    }
    public async Task MarkNotificationReadByTypeAsync(NotificationType type, string employeeId)
    {
      IEnumerable<NotificationDto> notification = await _notificationRepository
      .GetAllEmployeeNotificationsByTypeAsync(type, employeeId);

      foreach (NotificationDto n in notification)
      {
        n.IsRead = true;
        BuildIdempotencyKey(n.ToNotificationFromDto());
        await _notificationRepository.MarkAsReadAsync(n.ToNotificationFromDto());
      }
    }
    public async Task CreateAndDispatchAsync(Notification notification)
    {
      //Check if this persistent notifications has already been created.
      //Rule is Payroll and TaxUpload notifications cannot be marked read 
      //unless certain condititions hold true. This makes them 
      //susceptible to duplicate notifications being created thus polluting the database.
      //
      //This can be avoided by ensure the Insert db request is Idempotent. Build the 
      //key before querying the db
      BuildIdempotencyKey(notification);
      bool isPesistent = NotificationsRules.ShouldPersist(notification.Severity);
      if (isPesistent)
      {
        Notification? exists = await _notificationRepository.TryAndAquireAsync(notification.IdempotencyKey);
        if (exists != null) { return; }
      }
      //For other general notifications or it does not exist
      _ = await _notificationRepository.AddNotificationAsync(notification);
      _ = await _notificationRepository.Save();
      //Dispatch all notifications
      await _notificationDispatcher.DispatchNotificationAsync(notification);
    }

    public async Task CreateOrEnsureExistsAsync(Notification notification)
    {
      bool isPesistent = NotificationsRules.ShouldPersist(notification.Severity);
      if (isPesistent)
      {
        //Find if it already exists 
        BuildIdempotencyKey(notification);
        // var existing = await _notificationRepository.ExistsAsync(notification.Type, notification.EmployeeId, notification.Message, notification.Severity);
        Notification? exists = await _notificationRepository.TryAndAquireAsync(notification.IdempotencyKey);

        if (exists != null)
        {
          //Keep the Payroll and Tax Table uploads notifications persistent
          // notification.IsRead = true;
          return;
        }
      }
      //For other general notifications or it does not exist
      _ = await _notificationRepository.AddNotificationAsync(notification);
      _ = await _notificationRepository.Save();
      //Dispatch all notifications
      await _notificationDispatcher.DispatchNotificationAsync(notification);
    }
    private void BuildIdempotencyKey(Notification request)
    {
      string hashSource = $"{request.Type}:{request.EmployeeId}:{request.DeliveryChannel}:{request.Message.Trim()}";
      using SHA256 sha = SHA256.Create();
      byte[] bytes = Encoding.UTF8.GetBytes(hashSource);
      byte[] hash = SHA256.HashData(bytes);
      request.IdempotencyKey = Convert.ToHexString(hash);
    }
    public async Task TryCreateAndDispatch(Notification notification)
    {
      BuildIdempotencyKey(notification);
      bool isPersistent = NotificationsRules.ShouldPersist(notification.Severity);
      Notification? created = null;
      if (isPersistent)
      {
        created = await _notificationRepository.TryCreateUnreadAsync(notification);
      }
      else
      {
        _ = await _notificationRepository.AddNotificationAsync(notification);
      }

      if (created != null)
      {
        return;
      }
      _ = await _notificationRepository.AddNotificationAsync(notification);
      await _notificationDispatcher.DispatchNotificationAsync(notification);
      // return created?.ToNotificationDto() ?? null;
    }

  }
}
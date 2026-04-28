namespace HRConnect.Api.Services
{
  using HRConnect.Api.Models;
  using System.Threading.Tasks;
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.DTOs.Notification;
  using HRConnect.Api.Utils.Notification;
  using System.Text;
  using System.Security.Cryptography;
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
      var notifications = await _notificationRepository.GetAllEmployeeNotificationsByTypeAsync(type, employeeId);

      return notifications;
    }
    public async Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsBySeverityAsync(NotificationSeverity severity, string employeeId)
    {
      var notifications = await _notificationRepository.GetAllEmployeeNotificationsBySeverityAsync(employeeId, severity);

      return notifications;
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
      bool isPesistent = NotificationTypeRules.ShouldPersist(notification.Severity);
      if (isPesistent)
      {
        var exists = await _notificationRepository.TryAndAquireAsync(notification.IdempotencyKey, notification.DeliveryChannel);
      }
      //For other general notifications or it does not exist
      await _notificationRepository.AddNotificationAsync(notification);
      _ = await _notificationRepository.Save();
      //Dispatch all notifications
      await _notificationDispatcher.DispatchNotificationAsync(notification);
    }

    public async Task CreateOrEnsureExistsAsync(Notification notification)
    {
      bool isPesistent = NotificationTypeRules.ShouldPersist(notification.Severity);
      if (isPesistent)
      {
        //Find if it already exists 
        BuildIdempotencyKey(notification);
        // var existing = await _notificationRepository.ExistsAsync(notification.Type, notification.EmployeeId, notification.Message, notification.Severity);
        var existing = await _notificationRepository.TryAndAquireAsync(notification.IdempotencyKey, notification.DeliveryChannel);


        if (existing is true)
        {
          //Keep the Payroll and Tax Table uploads notifications persistent
          // notification.IsRead = true;
          return;
        }
      }
      //For other general notifications or it does not exist
      await _notificationRepository.AddNotificationAsync(notification);
      _ = await _notificationRepository.Save();
      //Dispatch all notifications
      await _notificationDispatcher.DispatchNotificationAsync(notification);
    }
    private void BuildIdempotencyKey(Notification request)
    {
      var hashSource = $"{request.Type}:{request.EmployeeId}:{request.DeliveryChannel}:{request.Message.Trim()}";
      using var sha = SHA256.Create();
      var bytes = Encoding.UTF8.GetBytes(hashSource);
      var hash = sha.ComputeHash(bytes);
      request.IdempotencyKey = Convert.ToHexString(hash);
    }
  }
}
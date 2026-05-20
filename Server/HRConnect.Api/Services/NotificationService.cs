namespace HRConnect.Api.Services
{
  using System.Security.Cryptography;
  using System.Text;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs.Notification;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Mappers.Notification;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils.Notification;

  public class NotificationService : INotificationService
  {
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IUserEmployeeHttpClient _userHttpClient;
    public NotificationService(
      INotificationRepository notificationRepository,
      INotificationDispatcher notificationDispatcher,
      IUserEmployeeHttpClient userHttpClient
    )
    {
      _notificationRepository = notificationRepository;
      _notificationDispatcher = notificationDispatcher;
      _userHttpClient = userHttpClient;
    }

    public async Task<IEnumerable<NotificationDto>> GetEmployeeNotificationsAsync(int userId)
    {

      string employeeId = await _userHttpClient.ResolveEmployeeIdFromUserIdAsync(userId);
      IEnumerable<Notification> notifications =
        await _notificationRepository.GetEmployeeNotificationsAsync(employeeId);
      return notifications.Select(n => n.ToNotificationDto());
    }

    public async Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsByTypeAsync(
      NotificationType type,
     int userId
    )
    {

      string employeeId = await _userHttpClient.ResolveEmployeeIdFromUserIdAsync(userId);
      IEnumerable<Notification> notifications =
        await _notificationRepository.GetAllEmployeeNotificationsByTypeAsync(type, employeeId);

      return notifications.Select(n => n.ToNotificationDto());
    }

    public async Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsBySeverityAsync(
      NotificationSeverity severity,
    int userId
    )
    {

      string employeeId = await _userHttpClient.ResolveEmployeeIdFromUserIdAsync(userId);
      IEnumerable<Notification> notifications =
        await _notificationRepository.GetAllEmployeeNotificationsBySeverityAsync(
          employeeId,
          severity
        );

      return notifications.Select(n => n.ToNotificationDto());
    }

    public async Task MarkBatchedNotificationsReadByTypeAsync(
      NotificationType type,
      List<string> employeeIds
    )
    {
      await _notificationRepository.MarkBatchAsReadAsync(employeeIds, type);
    }

    public async Task MarkNotificationReadByTypeAsync(NotificationType type, int userId)
    {

      string employeeId = await _userHttpClient.ResolveEmployeeIdFromUserIdAsync(userId);
      IEnumerable<Notification> notification =
        await _notificationRepository.GetAllEmployeeNotificationsByTypeAsync(type, employeeId);

      foreach (Notification n in notification)
      {
        n.IsRead = true;
        BuildIdempotencyKey(n);
        await _notificationRepository.MarkAsReadAsync(n);
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
        Notification? exists = await _notificationRepository.TryAndAquireAsync(
          notification.IdempotencyKey
        );
        if (exists != null)
        {
          return;
        }
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
        Notification? exists = await _notificationRepository.TryAndAquireAsync(
          notification.IdempotencyKey
        );

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
      string hashSource =
        $"{request.Type}:{request.EmployeeId}:{request.DeliveryChannel}:{request.Message.Trim()}";
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
        await _notificationDispatcher.DispatchNotificationAsync(created);
        return;
      }
      _ = await _notificationRepository.AddNotificationAsync(notification);
      await _notificationDispatcher.DispatchNotificationAsync(notification);
      // return created?.ToNotificationDto() ?? null;
    }

    public async Task<bool> DeleteAllReadAsync()
    {

      return await _notificationRepository.DeleteAllReadAsync();
    }

    public async Task<bool> DeleteAllByEmployeeIdAsync(int userId)
    {

      string employeeId = await _userHttpClient.ResolveEmployeeIdFromUserIdAsync(userId);
      return await _notificationRepository.DeleteAllByEmployeeId(employeeId);
    }

    public async Task<bool> DeleteNotificationByIdAsync(int userId, int id)
    {
      string employeeId = await _userHttpClient.ResolveEmployeeIdFromUserIdAsync(userId);

      var deletedEntry = await _notificationRepository.DeleteNotificationByIdAsync(employeeId, id);
      if (!deletedEntry)
        return false;

      return true;
    }

    // public async Task<string> ResolveEmployeeIdFromUserIdAsync(int userId)
    // {
    //   try
    //   {
    //     User? user = await _httpClient.GetFromJsonAsync<User>($"users/{userId}") ??
    //      throw new KeyNotFoundException($"User Not Found: ");

    //     EmployeeDto employee = await _httpClient.GetFromJsonAsync<EmployeeDto>($"employee/email/{user.Email}") ??
    //        throw new KeyNotFoundException($"User Not Found: ");

    //     return employee.EmployeeId;
    //   }

    //   catch (InvalidDataException ex)
    //   {
    //     throw new InvalidDataException($"No Employee Exists For This User: {ex?.Message}");
    //   }
    // }
  }
}

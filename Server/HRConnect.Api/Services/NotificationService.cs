namespace HRConnect.Api.Services
{
  using System.Security.Cryptography;
  using System.Text;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs.Employee;
  using HRConnect.Api.DTOs.Notification;
  using HRConnect.Api.DTOs.User;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Mappers.Notification;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils.Notification;
  using Quartz.Util;

  public class NotificationService : INotificationService
  {
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IUserHttpClient _userHttpClient;
    private readonly IEmployeeService _employeeService;
    public NotificationService(
      INotificationRepository notificationRepository,
      INotificationDispatcher notificationDispatcher,
      IUserHttpClient userHttpClient,
      IEmployeeService employeeService
    )
    {
      _notificationRepository = notificationRepository;
      _notificationDispatcher = notificationDispatcher;
      _userHttpClient = userHttpClient;
      _employeeService = employeeService;
    }

    public async Task<IEnumerable<NotificationDto>> GetEmployeeNotificationsAsync(int userId)
    {
      string employeeId = await ResolveEmployeeId(userId);
      IEnumerable<Notification> notifications =
        await _notificationRepository.GetEmployeeNotificationsAsync(employeeId);
      return notifications.Select(n => n.ToNotificationDto());
    }

    public async Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsByTypeAsync(
      NotificationType type,
     int userId
    )
    {
      string employeeId = await ResolveEmployeeId(userId);
      IEnumerable<Notification> notifications =
        await _notificationRepository.GetAllEmployeeNotificationsByTypeAsync(type, employeeId);

      return notifications.Select(n => n.ToNotificationDto());
    }

    public async Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsBySeverityAsync(
      NotificationSeverity severity, int userId)
    {
      string employeeId = await ResolveEmployeeId(userId);
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
      string employeeId = await ResolveEmployeeId(userId);
      IEnumerable<Notification> notification =
        await _notificationRepository.GetAllEmployeeNotificationsByTypeAsync(type, employeeId);

      foreach (Notification n in notification)
      {
        n.IsRead = true;
        BuildIdempotencyKey(n);
        await _notificationRepository.MarkAsReadAsync(n);
      }
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
      bool isWarning = NotificationsRules.RequiresAction(notification.Type);
      Notification? created = null;

      if (isPersistent)
      {
        created = await _notificationRepository.TryCreateUnreadAsync(notification);
      }
      {
        // #line 115 "(===================>NotificationService.cs)"
        //         Console.ForegroundColor = ConsoleColor.Blue;
        //         Console.WriteLine($"SHOULD update this notification");
        //         created = await _notificationRepository.AddNotificationAsync(notification);
        // #line default 
      }

      if (created != null)
      {

        await _notificationDispatcher.DispatchNotificationAsync(created);
        return;
      }
      _ = await _notificationRepository.AddNotificationAsync(notification);
      await _notificationDispatcher.DispatchNotificationAsync(notification);
    }

    public async Task<bool> DeleteAllReadAsync()
    {

      return await _notificationRepository.DeleteAllReadAsync();
    }
    public async Task MarkAllAsReadByUserId(int userId)
    {
      string employeeId = await ResolveEmployeeId(userId);
      if (employeeId.IsNullOrWhiteSpace())
        return;
      await _notificationRepository.MarkAllAsReadByEmployeeId(employeeId);
    }
    public async Task<bool> DeleteAllByEmployeeIdAsync(int userId)
    {
      string employeeId = await ResolveEmployeeId(userId);
      return await _notificationRepository.DeleteAllByEmployeeId(employeeId);
    }

    public async Task<bool> DeleteNotificationByIdAsync(int userId, int id)
    {
      string employeeId = await ResolveEmployeeId(userId);

      bool deletedEntry = await _notificationRepository.DeleteNotificationByIdAsync(employeeId, id);

      if (!deletedEntry)
        return false;

      return true;
    }

    private async Task<string> ResolveEmployeeId(int userId)
    {
      UserRegisterDto user = await _userHttpClient.ResolveUserFromId(userId);
      Console.WriteLine($"Email Being Used To Fetch Employee: {user.Email}");
      EmployeeDto? employee = await _employeeService.GetEmployeeByEmailAsync(user.Email);
      if (employee == null)
        return string.Empty;

      return employee.EmployeeId;
    }
  }
}
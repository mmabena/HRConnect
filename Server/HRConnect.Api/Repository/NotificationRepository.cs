namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Models;
  using HRConnect.Api.Data;
  using HRConnect.Api.Mappers.Notification;
  using HRConnect.Api.DTOs.Notification;
  using Microsoft.EntityFrameworkCore;

  // public readonly struct SaveResult
  // {
  //   public bool IsSuccess { get; init};
  //   public string? ResultMsg { get; init; }
  // }
  public class NotificationRepository : INotificationRepository
  {
    // Task <NotificationDto> CreateNotification
    private readonly ApplicationDBContext _context;
    public NotificationRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    public async Task AddNotificationAsync(Notification notification)
    {
      await _context.Notifications.AddAsync(notification);
      // var save = await Save();
    }

    public async Task Save()
    {
      await _context.SaveChangesAsync();

    }
    // Task AddNotificationBatchAsync(Notification notification);
    /// <summary>
    /// This metod acts as a deduplication safe guard when creating and dispatching 
    /// notifications. It is used as boolean check before notification storing
    /// </summary>
    /// <param name="type">The type of notification being created</param>
    /// <param name="dueDate">The date at which an action-based notification will be executed</param>
    /// <param name="dateTime">The date used to find notification creation</param>
    /// <returns>Notification Object</returns>
    public async Task<Notification?> ExistsAsync(NotificationType type, string employeeId, string? message, NotificationSeverity severity)
    {
      // return await _context.Notifications.FindAsync(type, message, employeeId, severity);
      return await _context.Notifications.Where(n =>
      (n.Type == type) &&
      (n.Message == message) &&
      (n.EmployeeId == employeeId) &&
      (n.Severity == severity) &&
      n.IsRead == false)//Avoid to duplicate unread messages
      .FirstOrDefaultAsync();
    }

    public async Task<bool> TryAndAquireAsync(string idempotencyKey, string deliveryChannel)
    {
      return await _context.Notifications.AsNoTracking()
        .AnyAsync(n =>
          (n.IdempotencyKey == idempotencyKey) &&
          (n.DeliveryChannel == deliveryChannel) &&
          n.IsRead == false);
    }
    public async Task<bool> MarkAsReadAsync(Notification notification)
    {
      var result = _context.Notifications.Update(notification);
      var saveResult = await Save();
      return saveResult.IsSuccess;
    }
    public async Task<IEnumerable<NotificationDto>> GetAllUnreadAsync(string? employeeId)
    {
      var notifications = await _context.Notifications.
            Where(n => !n.IsRead &&
            (n.EmployeeId == null || n.EmployeeId == employeeId))
      .OrderByDescending(n => n.CreatedAt).ToListAsync();
      // throw new NotImplementedException();
      return notifications.Select(n => n.ToNotificationDto()).ToList();
    }
    public async Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsByTypeAsync(NotificationType type, string employeeId)
    {
      var notifications = await _context.Notifications.Where(n =>
                  (n.EmployeeId == employeeId) &&
                  (n.Type == type)).ToListAsync();
      return notifications.Select(n => n.ToNotificationDto());
    }
    //Critical,Warning,Information
    public async Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsBySeverityAsync(string employeeId, NotificationSeverity severity)
    {
      var notifications = await _context.Notifications.Where(n =>
                  (n.EmployeeId == employeeId) &&
                  (n.Severity == severity)).ToListAsync();
      return notifications.Select(n => n.ToNotificationDto());
    }
  }
}
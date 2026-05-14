namespace HRConnect.Api.Repository
{
  using System.Linq;
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Models;
  using HRConnect.Api.Data;
  using HRConnect.Api.Mappers.Notification;
  using HRConnect.Api.DTOs.Notification;
  using Microsoft.EntityFrameworkCore;

  public class NotificationRepository : INotificationRepository
  {
    private readonly ApplicationDBContext _context;
    public NotificationRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    public async Task<Notification> AddNotificationAsync(Notification notification)
    {
      _ = await _context.Notifications.AddAsync(notification);
      _ = await Save();
      return notification;
    }

    public async Task<bool> Save()
    {
      if (await _context.SaveChangesAsync() > 0)
        return true;
      return false;
    }

    public async Task<IList<NotificationDto>> GetEmployeeNotificationsAsync(string employeeId)
    {
      var notifications = await _context.Notifications.AsNoTracking().Where(n =>
        (n.EmployeeId == employeeId) &&
        (n.DeliveryChannel == DeliveryChannel.InApp) &&
        !n.IsRead
        ).OrderByDescending(n => n.Type).ToListAsync();
      if (notifications == null)
        return [];
      return notifications.Select(n => n.ToNotificationDto()).ToList();
    }
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
      !n.IsRead)//Avoids duplicating unread messages
      .FirstOrDefaultAsync();
    }

    public async Task<Notification?> TryAndAquireAsync(string idempotencyKey)
    {
      return await _context.Notifications.AsNoTracking()
        .FirstOrDefaultAsync(n =>
          (n.IdempotencyKey == idempotencyKey) &&
          !n.IsRead);
    }
    public async Task MarkBatchAsReadAsync(List<string> employeeIds, NotificationType type)
    {
      //Prefer batching updating as SQL has a parameter limit of ~2100
      foreach (var idBatch in employeeIds.Chunk(500))
      {
        await _context.Notifications.Where(n =>
        idBatch.Contains(n.EmployeeId) &&
        (n.Type == type) &&
        !n.IsRead)
        .ExecuteUpdateAsync(s =>
        s.SetProperty(n => n.IsRead, true));
      }

      //Read Notifications are automatically deleted
      await DeleteAllReadByTypeAsync(type);
    }
    public async Task MarkAsReadAsync(Notification notification)
    {
      // _ = _context.Notifications.Update(notification);
      //attatching entity into the entity tracker 
      _ = _context.Attach(notification);

      notification.IsRead = true;

      _context.Entry(notification).Property(n => n.IsRead)
      .IsModified = true;

      //Read Notifications are automatically deleted
      //  await DeleteAllReadAsync();

      _ = await Save();
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
    public async Task<Notification?> TryCreateUnreadAsync(Notification notification)
    {
      using var tsx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

      var existsUnread = await TryAndAquireAsync(notification.IdempotencyKey);
      if (existsUnread == null)
        return existsUnread;

      await _context.Notifications.AddAsync(notification);
      await _context.SaveChangesAsync();

      await tsx.CommitAsync();
      return notification;
    }
    public async Task<bool> DeleteAllReadAsync()
    {
      return await _context.Notifications.Where(n => n.IsRead)
          .ExecuteDeleteAsync() > 0;
    }
    public async Task<bool> DeleteAllReadByTypeAsync(NotificationType type)
    {
      return await _context.Notifications.Where(n => n.IsRead &&
          n.Type == type)
          .ExecuteDeleteAsync() > 0;
    }

    public async Task<bool> DeleteAllByEmployeeId(string employeeId)
    {
      return await _context.Notifications.Where(n => n.EmployeeId == employeeId)
        .ExecuteDeleteAsync() > 0;
    }

  }
}
namespace HRConnect.Api.Repository
{
  using System.Linq;
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Models;
  using HRConnect.Api.Data;
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

    public async Task<IEnumerable<Notification>> GetEmployeeNotificationsAsync(string employeeId)
    {
      Console.WriteLine("========== Notification Repository ==========");
      Console.WriteLine($"EmployeeId received: '{employeeId}'");

      List<Notification> notifications = await _context.Notifications.AsNoTracking().Where(n =>
        (n.EmployeeId == employeeId) &&
        (n.DeliveryChannel == DeliveryChannel.InApp))
        .OrderBy(n => n.Severity)
        .ToListAsync();

      Console.WriteLine($"Notifications found: {notifications.Count}");
      foreach (var notification in notifications)
      {
        Console.WriteLine(
            $"Id: {notification.NotificationId} | " +
            $"Employee: {notification.EmployeeId} | " +
            $"Channel: {notification.DeliveryChannel} | " +
            $"Subject: {notification.Subject}");
      }

      Console.WriteLine("============================================");
      return notifications;
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
      foreach (string[] idBatch in employeeIds.Chunk(500))
      {
        await _context.Notifications.Where(n =>
        idBatch.Contains(n.EmployeeId) &&
        (n.Type == type) &&
        !n.IsRead)
        .ExecuteUpdateAsync(s =>
        s.SetProperty(n => n.IsRead, true));
      }
      await DeleteAllReadByTypeAsync(type);
      await _context.SaveChangesAsync();
    }
    public async Task MarkAsReadAsync(Notification notification)
    {
      //attatching entity into the entity tracker 
      _ = _context.Attach(notification);

      notification.IsRead = true;

      _context.Entry(notification).Property(n => n.IsRead)
      .IsModified = true;

      _ = await Save();
    }
    public async Task MarkAllAsReadByEmployeeId(string employeeId)
    {
      await _context.Notifications.Where(n =>
      (n.EmployeeId == employeeId) &&
      !new[] { /*NotificationType.Payroll,*/ NotificationType.TaxUpload }.Contains(n.Type)
      ).ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead,
      n => true));

      await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<Notification>> GetAllUnreadAsync(string? employeeId)
    {
      var notifications = await _context.Notifications.
            Where(n => !n.IsRead &&
            (n.EmployeeId == null || n.EmployeeId == employeeId))
      .OrderByDescending(n => n.CreatedAt).ToListAsync();
      return notifications;
    }
    public async Task<IEnumerable<Notification>> GetAllEmployeeNotificationsByTypeAsync(NotificationType type, string employeeId)
    {
      var notifications = await _context.Notifications.Where(n =>
                  (n.EmployeeId == employeeId) &&
                  (n.Type == type)).ToListAsync();
      return notifications;
    }
    public async Task<IEnumerable<Notification>> GetAllEmployeeNotificationsBySeverityAsync(string employeeId, NotificationSeverity severity)
    {
      var notifications = await _context.Notifications.Where(n =>
                  (n.EmployeeId == employeeId) &&
                  (n.Severity == severity)).ToListAsync();
      return notifications;
    }
    public async Task<Notification?> TryCreateUnreadAsync(Notification notification)
    {
      using var tsx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
      Console.WriteLine("========== TryCreateUnread ==========");
      Console.WriteLine($"Notification Type : {notification.Type}");
      Console.WriteLine($"EmployeeId        : {notification.EmployeeId}");
      Console.WriteLine($"Message           : {notification.Message}");
      Console.WriteLine($"Idempotency Key   : {notification.IdempotencyKey}");
      var existsUnread = await TryAndAquireAsync(notification.IdempotencyKey);
      if (existsUnread != null)
      {
        Console.WriteLine("Duplicate notification found.");
        Console.WriteLine($"Existing NotificationId : {existsUnread.NotificationId}");
        Console.WriteLine($"Existing CreatedAt      : {existsUnread.CreatedAt}");
        Console.WriteLine($"Existing IsRead         : {existsUnread.IsRead}");
        Console.WriteLine("Returning existing notification.");
        Console.WriteLine("====================================");

        return existsUnread;

      }
      Console.WriteLine("No duplicate found.");
      Console.WriteLine("Creating new notification...");

      _ = await _context.Notifications.AddAsync(notification);
      _ = await _context.SaveChangesAsync();

      await tsx.CommitAsync();
      Console.WriteLine($"Created NotificationId : {notification.NotificationId}");
      Console.WriteLine("====================================");
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
    public async Task<bool> DeleteNotificationByIdAsync(string employeeId, int id)
    {
      Notification? notification = await _context.Notifications.Where(n => n.EmployeeId == employeeId)
      .FirstOrDefaultAsync(n => n.NotificationId == id);

      if (notification == null)
        return false;

      _ = _context.Notifications.Remove(notification);
      _ = await _context.SaveChangesAsync();
      return true;
    }
  }
}
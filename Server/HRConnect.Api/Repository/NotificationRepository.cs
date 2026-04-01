namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Models;
  using HRConnect.Api.Data;
  using HRConnect.Api.Mappers.Notification;
  using HRConnect.Api.DTOs.Notification;
  using Microsoft.EntityFrameworkCore;

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
      await _context.SaveChangesAsync();
    }
    /// <summary>
    /// This metod acts as a deduplication safe guard when creating and dispatching 
    /// notifications. It is used as boolean check before notification storing
    /// </summary>
    /// <param name="type">The type of notification being created</param>
    /// <param name="dueDate">The date at which an action-based notification will be executed</param>
    /// <param name="dateTime">The date used to find notification creation</param>
    /// <returns></returns>
    public async Task<Notification?> ExistsAsync(NotificationType type, DateTime? dueDate, DateTime dateTime)
    {
      // return await _context.Notifications.AnyAsync(n =>
      // (n.Type == type) &&
      // (n.DueDate == null || n.DueDate == dueDate) &&
      // (n.CreatedAt == dateTime));
      return await _context.Notifications.FirstOrDefaultAsync(n =>
      (n.Type == type) &&
      (n.DueDate == null || n.DueDate == dueDate) &&
      (n.CreatedAt == dateTime));
    }
    public async Task<bool> MarkAsReadAsync(int id)
    {
      throw new NotImplementedException();
    }
    public async Task<IEnumerable<NotificationDto>> GetAllUnreadAsync(string? employeeId)
    {
      var notifications = await _context.Notifications.
            Where(n => !n.IsRead &&
            (n.EmployeeId == null || n.EmployeeId == employeeId) &&
      (n.DueDate == null || n.DueDate > DateTime.Now))
      .OrderByDescending(n => n.CreatedAt).ToListAsync();
      // throw new NotImplementedException();
      return notifications.Select(n => n.ToNotificationDto()).ToList();
    }
  }
}
namespace HRConnect.Api.Repositories
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.Notification;
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
      throw new NotImplementedException();
    }
    public async Task<bool> ExistsAsync(string type, DateTime executionDate, DateTime dateTime)
    {
      throw new NotImplementedException();
    }
    public async Task<bool> MarkAsReadAsync(int id)
    {
      throw new NotImplementedException();
    }
    public async Task<IEnumerable<NotificationDto>> GetAllUnreadAsync()
    {
      throw new NotImplementedException();
    }
  }
}
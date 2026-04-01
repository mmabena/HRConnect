namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.Models;
  public interface INotificationService
  {
    Task CreateAndDispatchAsync(Notification notification);
    Task CreateOrEnsureExistsAsync(Notification notification);
  }
}
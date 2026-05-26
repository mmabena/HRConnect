namespace HRConnect.Api.Interfaces.Notification
{
  using HRConnect.Api.DTOs.Notification;
  using HRConnect.Api.Models;

  public interface INotificationService
  {
    Task MarkNotificationReadByTypeAsync(NotificationType type, int userId);
    Task<IEnumerable<NotificationDto>> GetEmployeeNotificationsAsync(int userId);
    ///<summary>
    /// Function is used to mark a batch of Notifications Read. This is done on a Notification Type
    /// <see cref="NotificationType"/>
    /// This is slightly faster than <see cref="MarkBatchedNotificationsReadByTypeAsync(NotificationType,
    /// List{string})"> as there is no overhead to find employee Id from userId
    /// <paramref name="type">Notification Types to be marked read</paramref>
    /// <paramref name="employeeIds">List (Batch of +-500 ) of employee Ids to delete.</paramref>
    /// </summary>
    Task MarkBatchedNotificationsReadByTypeAsync(NotificationType type, List<string> employeeIds);
    Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsBySeverityAsync(
      NotificationSeverity severity,
      int userId
    );
    ///<summary>
    /// Function is used to mark a batch of Notifications Read. This is done on a Notification Type
    /// <see cref="NotificationType"/>
    /// <paramref name="type">Notification Types to be marked read</paramref>
    /// <paramref name="userId">List (Batch of +-500 ) of user Ids to delete.</paramref>
    /// </summary>
    Task<IEnumerable<NotificationDto>> GetAllEmployeeNotificationsByTypeAsync
      (NotificationType type, int userId);
    ///<summary>
    /// This is used to conditionally create a notification in the database.
    /// To prevent duplication of notifications, Idempotency Keys (<see
    /// cref="NotificationService.BuildIdempotencyKey(Notification)") are used to ensure uniqueness.
    /// This is not a database level validation 
    ///</summary>
    Task TryCreateAndDispatch(Notification notification);
    Task<bool> DeleteAllReadAsync();
    Task<bool> DeleteAllByEmployeeIdAsync(int userId);
    Task<bool> DeleteNotificationByIdAsync(int userId, int id);
    Task MarkAllAsReadByUserId(int userId);
  }
}
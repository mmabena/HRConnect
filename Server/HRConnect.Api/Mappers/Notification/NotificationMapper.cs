namespace HRConnect.Api.Mappers.Notification
{
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Notification;
  public static class NotificationMapper
  {
    public static NotificationDto ToNotificationDto(this Notification notificationModel)
    {
      return new NotificationDto { };
    }
    public static Notification ToNotificationFromDto(this NotificationDto dto)
    {
      return new Notification { };
    }
  }
}
namespace HRConnect.Api.Mappers.Notification
{
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Notification;
  public static class NotificationMapper
  {
    public static NotificationDto ToNotificationDto(this Notification notificationModel)
    {
      return new NotificationDto
      {
        Message = notificationModel.Message,
        IsRead = notificationModel.IsRead,
        Type = notificationModel.Type,
        Severity = notificationModel.Severity,
        DueDate = notificationModel.DueDate,
        DeliveryChannel = notificationModel.DeliveryChannel,
        CreatedAt = notificationModel.CreatedAt
      };
    }
    public static Notification ToNotificationFromDto(this NotificationDto dto)
    {
      return new Notification
      {
        Message = dto.Message,
        Type = dto.Type,
        Severity = dto.Severity,
        DueDate = dto.DueDate,
        IsRead = dto.IsRead,
        DeliveryChannel = dto.DeliveryChannel,
        IdempotencyKey = dto.IdempotencyKey
      };
    }
    public static Notification ToNotificationFromCreateDto(this CreateNotificationDto dto)
    {
      return new Notification
      {
        Message = dto.Message,
        Type = dto.Type,
        Severity = dto.Severity,
        EmployeeId = dto.EmployeeId,
        DueDate = dto.DueDate,
        DeliveryChannel = dto.DeliveryChannel,
      };
    }
  }
}
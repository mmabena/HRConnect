namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs.Notification;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Models;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;


  [ApiController]
  [Route("api/notifications")]
  public class NotificationController : ControllerBase
  {
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;
    public NotificationController(INotificationService notificationService,
    IUserService userService)
    {
      _notificationService = notificationService;
      _userService = userService;
    }

    [Authorize(Roles = "SuperUser")]
    [HttpGet("payroll/{userId}")]
    public async Task<ActionResult<IList<NotificationDto>>> GetAllPayrollNotifications(int userId)
    {
      var notifications = await _notificationService.GetAllEmployeeNotificationsByTypeAsync(NotificationType.Payroll, $"{userId}");

      if (!notifications.Any())
        return NotFound();
      return Ok(notifications);
    }

    [HttpGet("{employeeId}")]
    public async Task<ActionResult<IList<NotificationDto>>> GetEmployeeNotifications(string employeeId)
    {
      var notifications = await _notificationService.GetEmployeeNotificationsAsync(employeeId);
      if (!notifications.Any())
        return NotFound();
      return Ok(notifications);
    }

  }
}
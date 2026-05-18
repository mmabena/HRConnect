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
    // private readonly IUserService _userService;
    public NotificationController(INotificationService notificationService
    /*IUserService userService*/)
    {
      _notificationService = notificationService;
      // _userService = userService;
    }

    [Authorize(Roles = "SuperUser")]
    [HttpGet("payroll/{employeeId}")]
    public async Task<IActionResult> GetAllPayrollNotifications(string employeeId)
    {
      var notifications = await _notificationService.GetAllEmployeeNotificationsByTypeAsync(NotificationType.Payroll,
      employeeId);

      if (!notifications.Any())
        return NotFound("No Notifications To Show");
      return Ok(notifications);
    }

    [Authorize(Roles = "SuperUser")]
    [HttpGet("tax/{employeeId}")]
    public async Task<IActionResult> GetAllTaxNotifications(string employeeId)
    {
      var notifications = await _notificationService.GetAllEmployeeNotificationsByTypeAsync(NotificationType.TaxUpload
      , employeeId);

      if (!notifications.Any())
        return NotFound($"No Notifications To Show");
      return Ok(notifications);
    }

    [HttpGet("{employeeId}")]
    public async Task<IActionResult> GetEmployeeNotifications(string employeeId)
    {
      var notifications = await _notificationService.GetEmployeeNotificationsAsync(employeeId);
      if (!notifications.Any())
        return NotFound();
      return Ok(notifications);
    }

    [HttpDelete("{employeeId}/{id}")]
    public async Task<IActionResult> DeleteNotificationById(string employeeId, int id)
    {
      bool isDeleted = await _notificationService.DeleteNotificationByIdAsync(employeeId, id);
      if (!isDeleted)
      { return NotFound(); }
      return Ok("Notification Deleted");
    }
  }
}
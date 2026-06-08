namespace HRConnect.Api.Utils.Jobs.Notification
{
  using Quartz;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.DTOs.Notification;
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Employee;

  [DisallowConcurrentExecution]
  public class NotificationJob : IJob
  {
    private readonly IJobScheduleService _jobScheduleService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IEmployeeService _employeeService;
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;
    private static readonly int DAYS_TO_ROLLOVER_NOTIFICATION = 5;
    public NotificationJob(IJobScheduleService jobScheduleService, INotificationFactory notificationFactory, IUserService userService,
    IEmployeeService employeeService, INotificationService notificationService)
    {
      _jobScheduleService = jobScheduleService;
      _notificationFactory = notificationFactory;
      _userService = userService;
      _employeeService = employeeService;
      _notificationService = notificationService;
    }

    public async Task<List<string>> OrganiseSuperUsersAsync()
    {
      List<User> users = await _userService.GetAllUsersAsync();

      users = users.FindAll(u => u.Role == Userrole.SuperUser ||
      u.TempRole == Userrole.SuperUser);
      List<string> employeeIds = [];

      foreach (User u in users)
      {
        EmployeeDto? e = await _employeeService.GetEmployeeByEmailAsync(u.Email);
        if (e is not null)
        {
          employeeIds.Add(e.EmployeeId);
        }
      }
      await _notificationService.MarkBatchedNotificationsReadByTypeAsync(NotificationType.Payroll, employeeIds);
      return employeeIds;
    }

    public async Task Execute(IJobExecutionContext context)
    {
      DateTimeOffset? payrollExecutionDate = await _jobScheduleService.GetNextJobScheduleAsync("PayrollRolloverJob");

      if (payrollExecutionDate == null)
        return;

      DateTimeOffset now = DateTimeOffset.Now;
      double daysUntilRollover = (payrollExecutionDate.Value.Date - now.Date).Days;

      Console.WriteLine($"----===... => DAYS TO ROLL OVER {daysUntilRollover}");

      if (daysUntilRollover >= 1 &&
       daysUntilRollover <= DAYS_TO_ROLLOVER_NOTIFICATION)
      {
        var superUserIds = await _userService.OrganiseSuperUsersAsync();
        foreach (var su in superUserIds)
        {
          CreateNotificationDto notification = new()
          {
            Message = $"Finalise Payroll. Payroll Will Rollover In {daysUntilRollover} days",
            Subject = "Finalise Payroll",
            Severity = NotificationSeverity.Critical,
            Type = NotificationType.Payroll,
            DeliveryChannel = DeliveryChannel.InApp,
            DueDate = payrollExecutionDate.Value.DateTime,
            EmployeeId = $"{su}"
          };
          await _notificationFactory.ProduceNotificationAsync(notification);
        }
      }
    }
  }
}
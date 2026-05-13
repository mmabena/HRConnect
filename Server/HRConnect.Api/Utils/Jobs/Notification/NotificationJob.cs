namespace HRConnect.Api.Utils.Jobs.Notification
{
  using Quartz;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.DTOs.Notification;
  using HRConnect.Api.Models;

  // Prevent multiple of these jobs from running concurrently
  [DisallowConcurrentExecution]
  public class NotificationJob : IJob
  {
    private readonly IJobScheduleService _jobScheduleService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IEmployeeService _employeeService;
    private readonly IUserService _userService;
    // private static readonly int DAYS_TO_ROLLOVER_NOTIFICATION = 5;
    public NotificationJob(IJobScheduleService jobScheduleService, INotificationFactory notificationFactory, IUserService userService,
    IEmployeeService employeeService)
    {
      _jobScheduleService = jobScheduleService;
      _notificationFactory = notificationFactory;
      _userService = userService;
      _employeeService = employeeService;
    }

    public async Task<List<string>> OrganiseSuperUsersAsync()
    {
      var users = await _userService.GetAllUsersAsync();

      //Only returns users with SuperUser role
      users = users.FindAll(u => u.Role == UserRole.SuperUser);
      List<string> employeeIds = new();

      foreach (var u in users)
      {
        var e = await _employeeService.GetEmployeeByEmailAsync(u.Email);
        if (e is not null)
          employeeIds.Add(e.EmployeeId);
      }
      return employeeIds;
    }

    public async Task Execute(IJobExecutionContext context)
    {
      var payrollExecutionDate = await _jobScheduleService.GetNextJobScheduleAsync("PayrollRolloverJob");

      if (payrollExecutionDate == null)
        return; //No days found

      // Swap this in when pushing to main 
      // int daysUntilRollover = (payrollExecutionDate.Value.Date - DateTime.Now).Days;

      int secondsUntilRollover = (DateTime.Now - payrollExecutionDate.Value).Seconds;
      Console.WriteLine($"====SECONDS UNTIL ROLLOVER {secondsUntilRollover}");
      Console.WriteLine($"====SECONDS FROM PAYROLL EXECUTION DATE {payrollExecutionDate.Value.Second}");
      Console.WriteLine($"====Now {DateTime.Now.Second}");
      if (secondsUntilRollover > 0)
      {
        var superUserIds = await OrganiseSuperUsersAsync();
        foreach (var su in superUserIds)
        {
          //every user in these iterations is a super user
          var notification = new CreateNotificationDto
          {
            Message = $"Finalise Payroll. Payroll Will Rollover In {secondsUntilRollover}",
            Subject = "Finalise Payroll",
            Severity = NotificationSeverity.Critical,
            Type = NotificationType.Payroll,
            DeliveryChannel = DeliveryChannel.InApp,
            DueDate = payrollExecutionDate,
            EmployeeId = $"{su}"
          };
          await _notificationFactory.ProduceNotificationAsync(notification);
        }
      }
    }
  }
}
namespace HRConnect.Api.Utils.Jobs.Notification
{
  using global::Quartz;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.DTOs.Notification;
  using HRConnect.Api.Models;
  using HRConnect.Api.Data;

  // Prevent multiple of these jobs from running concurrently
  [DisallowConcurrentExecution]

  public class NotificationJob : IJob
  {
    private readonly IJobScheduleService _jobScheduleService;
    private readonly INotificationFactory _notificationFactory;
    private readonly ApplicationDBContext _context;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IUserService _userService;
    // private static readonly int DAYS_TO_ROLLOVER_NOTIFICATION = 5;
    public NotificationJob(IJobScheduleService jobScheduleService, INotificationFactory notificationFactory, INotificationDispatcher notificationDispatcher, IUserService userService, ApplicationDBContext context)
    {
      _jobScheduleService = jobScheduleService;
      _notificationFactory = notificationFactory;
      _notificationDispatcher = notificationDispatcher;
      _userService = userService;
      _context = context;
    }

    public async Task<IList<Employee>> OrganiseSuperUsersAsync()
    {
      var users = await _userService.GetAllUsersAsync();

      //Only returns users with SuperUser role
      users = users.FindAll(u => u.Role == UserRole.SuperUser);
      Employee employees = new Array();

    }

    public async Task Execute(IJobExecutionContext context)
    {
      var payrollExecutionDate = await _jobScheduleService.GetNextJobScheduleAsync("PayrollRolloverJob");

      if (payrollExecutionDate == null)
        return; //No days found

      // Swap this in when pushing to main 
      // int daysUntilRollover = (payrollExecutionDate.Value.Date - DateTime.Now).Days;

      int secondsUntilRollover = (payrollExecutionDate.Value - DateTime.Now).Seconds;
      if (secondsUntilRollover > 0)
      {
        var superUser = await OrganiseSuperUsersAsync();
        foreach (var su in superUser)
        {
          //every user in these iterations is a super user
          var notification = new CreateNotificationDto
          {
            Message = $"Finalise Payroll. Payroll Will Rollover In {secondsUntilRollover}",
            Severity = NotificationSeverity.Critical,
            Type = NotificationType.Payroll,
            DeliveryChannel = "InApp",
            DueDate = payrollExecutionDate,
            EmployeeId = $"{su.UserId}"
          };
          await _notificationFactory.ProduceNotificationAsync(notification);
        }
      }
    }
  }
}
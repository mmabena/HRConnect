namespace HRConnect.Api.Utils.Jobs.Notification
{
  using global::Quartz;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Models;

  // Prevent multiple of these jobs from running concurrently
  [DisallowConcurrentExecution]

  public class NotificationJob : IJob
  {
    private readonly IJobScheduleService _jobScheduleService;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotificationDispatcher _notificationDispatcher;
    private static readonly int DAYS_TO_ROLLOVER_NOTIFICATION = 5;
    public NotificationJob(IJobScheduleService jobScheduleService, INotificationFactory notificationFactory, INotificationDispatcher notificationDispatcher)
    {
      _jobScheduleService = jobScheduleService;
      _notificationFactory = notificationFactory;
      _notificationDispatcher = notificationDispatcher;
    }
    public async Task Execute(IJobExecutionContext context)
    {
      var payrollExecutionDate = await _jobScheduleService.GetNextJobScheduleAsync("PayrollRolloverJob");

      Console.WriteLine($"NEXT EXECUTION Date for Payroll Rollover job {payrollExecutionDate}");

      if (payrollExecutionDate == null)
        return; //No days found

      //Swap this in when pushing to main 
      // int daysUntilRollover = (payrollExecutionDate.Value.Date - DateTime.Now).Days;

      int secondsUntilRollover = (DateTime.Now - payrollExecutionDate.Value).Seconds;

      if (secondsUntilRollover > 0)
      {
        // var superUsers = await _
        var notification = new Notification
        {
          Message = $"Finalise Payroll. Payroll Will Rollover In {secondsUntilRollover}",
          IsRead = false,
          Severity = NotificationSeverity.Critical,
          Type = NotificationType.Payroll,
          CreatedAt = DateTime.Now,
          DeliveryChannel = "InApp"
        };
        // await _notificationFactory.ProduceNotificationAsync(notification);
        // await _notificationDispatcher.DispatchNotificationAsync(notification);
      }

    }

  }
}
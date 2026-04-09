namespace HRConnect.Api.Utils.Jobs.Notification
{
  using global::Quartz;
  using HRConnect.Api.Interfaces;

  // Prevent multiple of these jobs from running concurrently
  [DisallowConcurrentExecution]

  public class Notificationjob : IJob
  {
    private readonly IJobScheduleService _jobScheduleService;
    public Notificationjob(IJobScheduleService jobScheduleService)
    {
      _jobScheduleService = jobScheduleService;
    }
    public async Task Execute(IJobExecutionContext context)
    {
      var payrollExecutionDate = await _jobScheduleService.GetNextJobScheduleAsync("PayrollRollover");
    }

  }
}
namespace HRConnect.Api.Services
{
  using System.Linq;
  using HRConnect.Api.Interfaces;
  using Quartz;
  public class JobScheduleService : IJobScheduleService
  {
    private readonly ISchedulerFactory _schedulerFactory;
    public JobScheduleService(ISchedulerFactory schedulerFactory)
    {
      _schedulerFactory = schedulerFactory;
    }
    public async Task<DateTime?> GetNextJobScheduleAsync(string jobName)
    {
      var scheduler = await _schedulerFactory.GetScheduler();

      var triggers = await scheduler.GetTriggersOfJob(new JobKey(jobName));

      //get the date set to drigger
      var trigger = triggers.OrderBy(t => t.GetNextFireTimeUtc()).FirstOrDefault();

      if (trigger != null)
      {
        return trigger.GetNextFireTimeUtc()?.DateTime;
      }
      return null;
    }
  }
}
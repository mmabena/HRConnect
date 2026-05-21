
namespace HRConnect.Api.Interfaces
{
  public interface IJobScheduleService
  {
    Task<DateTimeOffset?> GetNextJobScheduleAsync(string jobName);
  }
}
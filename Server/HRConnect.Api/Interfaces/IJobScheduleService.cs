
namespace HRConnect.Api.Interfaces
{
  public interface IJobScheduleService
  {
    Task<DateTime?> GetNextJobScheduleAsync(string jobName);
  }
}
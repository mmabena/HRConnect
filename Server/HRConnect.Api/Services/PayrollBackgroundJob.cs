namespace HRConnect.Api.Services
{
  using System.Threading;
  using System.Threading.Tasks;

  public class PayrollBackgroundJob : BackgroundService
  {
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public PayrollBackgroundJob(IServiceScopeFactory serviceScopeFactory)
    {
      _serviceScopeFactory = serviceScopeFactory;
    }

    //Override to be able to create a scope to run payroll run serivce as a background job
    //This is job schedular that uses polling to check system clock every hour
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      //stoppingToken is provided by a background service
      while (!stoppingToken.IsCancellationRequested)
      {
        // Periodically checking the clock to see what's the current time 
        // to run the backround process.Checks on an hourly basis
        // this background process isn't sensitive and checks on the system shouldn't
        // happen too frequently 
        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);

        using var scope = _serviceScopeFactory.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<PayrollRolloverService>();
        //call roll over method in the background
        await service.ExecuteRolloverAsync();
      }
    }
  }
}
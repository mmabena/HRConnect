namespace HRConnect.Api.Services
{
    using HRConnect.Api.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class LeaveAutomationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LeaveAutomationBackgroundService> _logger;

        /// <summary>
        /// Defines a background service that runs daily to automate leave carryover notifications and annual reset processes,
        /// utilizing scoped services to perform the necessary operations while ensuring proper logging of service start, execution, 
        /// and error handling, and scheduling the next run to occur at midnight UTC each day.
        /// </summary>
        private static readonly Action<ILogger, Exception?> _serviceStarted =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1000, nameof(LeaveAutomationBackgroundService)),
                "Leave automation service started.");
        /// <summary>
        /// Defines a log message for when the leave automation background service is running its carryover and reset checks,
        /// providing information about the execution of these checks and allowing for monitoring of the service's activity
        /// </summary>
        private static readonly Action<ILogger, Exception?> _runningCheck =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1001, nameof(LeaveAutomationBackgroundService)),
                "Running carryover + reset check.");
        /// <summary>
        /// Defines a log message for when an error occurs during the execution of the leave automation background service,
        /// allowing for error tracking and troubleshooting by logging the exception details at the error level.
        /// </summary>
        private static readonly Action<ILogger, Exception?> _errorOccurred =
            LoggerMessage.Define(
                LogLevel.Error,
                new EventId(1002, nameof(LeaveAutomationBackgroundService)),
                "Error running leave automation.");
        /// <summary>
        /// Initializes a new instance of the LeaveAutomationBackgroundService class, 
        /// injecting the necessary dependencies such as IServiceScopeFactory for creating scoped service instances and ILogger for logging service activity and errors,
        /// to set up the background service for automating leave carryover notifications and annual reset processes while ensuring proper logging and error handling throughout its execution.
        /// </summary>
        /// <param name="scopeFactory"></param>
        /// <param name="logger"></param>
        public LeaveAutomationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<LeaveAutomationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        /// <summary>
        /// Executes the background service, running an infinite loop that checks for cancellation requests and performs the carryover notification and annual reset processes,
        /// while logging the service start and execution, and handling any exceptions that may occur during the execution of these processes, 
        /// and scheduling the next run to occur at midnight UTC each day by calculating the delay until the next run and awaiting it with the cancellation token.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _serviceStarted(_logger, null);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var leaveProcessingService = scope.ServiceProvider
                        .GetRequiredService<ILeaveProcessingService>();

                    _runningCheck(_logger, null);

                    await leaveProcessingService.ProcessCarryOverNotificationAsync();
                    await leaveProcessingService.ProcessAnnualResetAsync();
                }
                catch (Exception ex)
                {
                    _errorOccurred(_logger, ex);
                }

                // Run once every 24 hours (Midnight precise)
                var nextRun = DateTime.UtcNow.Date.AddDays(1);
                var delay = nextRun - DateTime.UtcNow;

                await Task.Delay(delay, stoppingToken);

            }
        }
    }
}

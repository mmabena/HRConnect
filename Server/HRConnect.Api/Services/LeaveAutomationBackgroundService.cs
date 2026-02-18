
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

        // Precompiled log messages (CA1848 compliant)
        private static readonly Action<ILogger, Exception?> _serviceStarted =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1000, nameof(LeaveAutomationBackgroundService)),
                "Leave automation service started.");

        private static readonly Action<ILogger, Exception?> _runningCheck =
            LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1001, nameof(LeaveAutomationBackgroundService)),
                "Running carryover + reset check.");

        private static readonly Action<ILogger, Exception?> _errorOccurred =
            LoggerMessage.Define(
                LogLevel.Error,
                new EventId(1002, nameof(LeaveAutomationBackgroundService)),
                "Error running leave automation.");

        public LeaveAutomationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<LeaveAutomationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _serviceStarted(_logger, null);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider
                        .GetRequiredService<IEmployeeEntitlementService>();

                    _runningCheck(_logger, null);

                    await service.ProcessCarryOverNotificationAsync();
                    await service.ProcessAnnualResetAsync();
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

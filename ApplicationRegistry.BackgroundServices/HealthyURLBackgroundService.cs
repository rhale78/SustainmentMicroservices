using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.BackgroundServices
{
    public class HealthyURLBackgroundService : URLHealthCheckBackgroundServiceBase
    {
        public HealthyURLBackgroundService(IConfiguration configuration) : base(configuration, httpClient: null)
        {
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TimeSpan delayForHealthyCheck = Configuration.GetTimespanValue("HealthyCheckDelay", new TimeSpan(0, 0, 30));
            await RunURLCheckBackgroundService(healthCheck: true, "healthy", delayForHealthyCheck, stoppingToken).ConfigureAwait(false);
        }
    }
}

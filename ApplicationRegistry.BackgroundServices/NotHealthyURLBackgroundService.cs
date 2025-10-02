using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.BackgroundServices
{
    public class NotHealthyURLBackgroundService : URLHealthCheckBackgroundServiceBase
    {
        public NotHealthyURLBackgroundService(IConfiguration configuration) : base(configuration, httpClient: null)
        {
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TimeSpan delayForNotHealthyCheck = Configuration.GetTimespanValue("ShortCheckDelay", new TimeSpan(0, 0, 10));
            await RunURLCheckBackgroundService(healthCheck: false, "not healthy", delayForNotHealthyCheck, stoppingToken).ConfigureAwait(false);
        }
    }
}

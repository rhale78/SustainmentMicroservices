using ApplicationRegistry.DomainModels;
using ApplicationRegistry.Services;
using Log;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ApplicationRegistry.BackgroundServices
{
    public abstract class URLHealthCheckBackgroundServiceBase : BackgroundService
    {
        protected static readonly LogInstance Log = LogInstance.CreateLog();
        protected static IConfiguration Configuration { get; set; }
        protected HttpClient HttpClient { get; set; }   //RSH 2/8/24 - not a fan of this - should pull from http client factory
        protected IApplicationDiscoveryService ApplicationDiscoveryService { get; set; }

        public URLHealthCheckBackgroundServiceBase(IConfiguration configuration, HttpClient httpClient)
        {
            Configuration = configuration;
            ApplicationDiscoveryService = new ApplicationDiscoveryService(configuration);

            HttpClient = httpClient ?? new HttpClient();
        }

        protected abstract override Task ExecuteAsync(CancellationToken stoppingToken);

        protected async Task RunURLCheckBackgroundService(bool healthCheck, string healthString, TimeSpan healthCheckDelay, CancellationToken stoppingToken)
        {
            do
            {
                try
                {
                    Log.LogInformation("Checking {healthString} URLs", healthString);
                    List<ApplicationDiscoveryURL> urls = await GetMicroserviceURLs(healthCheck).ConfigureAwait(false);

                    await CheckAndUpdateURLHealthStatus(urls, stoppingToken).ConfigureAwait(false);
                    await Task.Delay(healthCheckDelay, stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.LogException(ex);
                }
            } while (!stoppingToken.IsCancellationRequested);
        }

        protected async Task<List<ApplicationDiscoveryURL>> GetMicroserviceURLs(bool getHealthyURLs)
        {
            DateTimeOffset outdatedURLStatusCheck = DateTimeOffset.UtcNow - Configuration.GetTimespanValue("DiscoveryURLHealthStatusTimespan", new TimeSpan(0, 0, 30));
            return await ApplicationDiscoveryService.GetHealthyOrUnhealthyURLs(getHealthyURLs, outdatedURLStatusCheck, true, true).ConfigureAwait(false);
        }

        protected async Task CheckAndUpdateURLHealthStatus(List<ApplicationDiscoveryURL> urls, CancellationToken cancellationToken)
        {
            foreach (ApplicationDiscoveryURL urlItem in urls)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                bool healthStatusChanged = false;
                string healthStatus = string.Empty;
                string healthURL = urlItem.Port != null && urlItem.Port >= 0 ? $"{urlItem.URL}:{urlItem.Port}/microserviceHealth" : $"{urlItem.URL}/microserviceHealth";

                try
                {
                    Log.LogDebug("Checking URL {healthURL}", healthURL);
                    string healthStatusRaw = await HttpClient.GetStringAsync(healthURL).ConfigureAwait(false);
                    string healthStatusPlus = string.Empty;

                    if (healthStatusRaw.Contains("Status"))
                    {
                        healthStatusPlus = healthStatusRaw[(healthStatusRaw.IndexOf("Status") + 10)..];
                        healthStatus = healthStatusPlus[..healthStatusPlus.IndexOf("\"")];
                        Log.LogDebug("URL {healthURL} returned status of {healthStatus}", healthURL, healthStatus);
                    }
                    else
                    {
                        //RSH 2/8/24 - warning that the health check is incorrect - using this for testing
                        healthStatus = healthStatusRaw;
                        Log.LogDebug("URL {healthURL} returned status of {healthStatus}", healthURL, healthStatus);
                    }

                }
                catch (HttpRequestException ex)
                {
                    Log.LogDebug("{healthURL} is down", healthURL);
                    healthStatus = "Down";
                }
                catch (Exception ex)
                {
                    Log.LogException(ex);
                    continue;
                }

                if (!string.IsNullOrEmpty(healthStatus))
                {
                    healthStatusChanged = DidHealthStatusChange(urlItem, healthStatus);

                    urlItem.HealthStatus = healthStatus;
                    urlItem.LastHealthStatusCheckDateTime = DateTimeOffset.UtcNow;
                }

                if (healthStatusChanged)
                {
                    await ApplicationDiscoveryService.UpdateURL(urlItem).ConfigureAwait(false);
                }
            }
        }

        protected bool DidHealthStatusChange(ApplicationDiscoveryURL urlItem, string healthStatus)
        {
            if (!string.Equals(urlItem.HealthStatus, healthStatus, StringComparison.OrdinalIgnoreCase) || urlItem.LastHealthStatusCheckDateTime == null)
            {
                Log.LogDebug("URL {urlItem.URL} health status changed from {urlItem.HealthStatus} to {healthStatus}", urlItem.URL, urlItem.HealthStatus, healthStatus);
                return true;
            }
            if (urlItem.LastHealthStatusCheckDateTime != null)
            {
                DateTimeOffset outdatedURLStatusCheck = DateTimeOffset.UtcNow - Configuration.GetTimespanValue("DiscoveryURLHealthStatusTimespan", new TimeSpan(0, 0, 30));

                if (urlItem.LastHealthStatusCheckDateTime < outdatedURLStatusCheck)
                {
                    Log.LogDebug("URL {urlItem.URL} expired last health check.  Updating with latest status to {healthStatus}", urlItem.URL, healthStatus);
                    return true;
                }
            }
            return false;
        }
    }
}

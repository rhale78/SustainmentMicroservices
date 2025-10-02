using ApplicationRegistry.Common;
using ApplicationRegistry.DomainModels;
using ApplicationRegistry.Interfaces;
using ApplicationRegistry.Model;
using ApplicationRegistry.Repository;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Services
{
    public class ApplicationDiscoveryService : IApplicationDiscoveryService
    {
        public IConfiguration Configuration { get; }
        public IApplicationDiscoveryRepository ApplicationDiscoveryRepository { get; }

        private IApplicationRegistryService ApplicationRegistryService { get; }
        public ApplicationDiscoveryService(IConfiguration configuration, IApplicationDiscoveryRepository applicationDiscoveryRepository = null)
        {
            Configuration = configuration;
            ApplicationDiscoveryRepository = applicationDiscoveryRepository ?? new ApplicationDiscoveryRepository(Configuration);
            ApplicationRegistryService = new ApplicationRegistryService(configuration, this);// applicationRegistryService;
        }

        public async Task RegisterSelf(ApplicationRegistryResult applicationRegistryResult)
        {
            List<ApplicationDiscoveryEntry> entries = Helpers.GetDiscoveryEntries(applicationRegistryResult.ApplicationInstanceID, applicationRegistryResult.ApplicationVersionID);
            foreach (ApplicationDiscoveryEntry entry in entries)
            {
                entry.ApplicationVersionID = applicationRegistryResult.ApplicationVersionID;
                entry.ApplicationInstanceID = applicationRegistryResult.ApplicationInstanceID;
                await AddDiscoveryRecords(entry).ConfigureAwait(false);
            }
        }
        public async Task SetURLsDown(int applicationInstanceID)
        {
            await ApplicationDiscoveryRepository.SetURLsDown(applicationInstanceID).ConfigureAwait(false);
        }

        public async Task<List<string>> GetURLsForFriendlyName(string friendlyName, bool limitToHttps)
        {
            List<(DomainModels.ApplicationDiscoveryURL, string)> urls = new List<(DomainModels.ApplicationDiscoveryURL, string)>();
            ApplicationDiscoveryItem applicationDiscoveryItem = await ApplicationDiscoveryRepository.GetDiscoveryItemByFriendlyName(friendlyName).ConfigureAwait(false);
            if (applicationDiscoveryItem != null)
            {
                foreach (ApplicationDiscoveryURLApplicationDiscoveryItem item in applicationDiscoveryItem.ApplicationDiscoveryURLApplicationDiscoveryItems)
                {
                    if (limitToHttps)
                    {
                        if (item.ApplicationDiscoveryURL.URL.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            urls.Add((item.ApplicationDiscoveryURL, applicationDiscoveryItem.ControllerRoute));
                        }
                    }
                    else
                    {
                        if (Configuration.GetBoolValueWithDefault("IgnoreDiscoveryHttpsURLs", false))
                        {
                            urls.Add((item.ApplicationDiscoveryURL, applicationDiscoveryItem.ControllerRoute));
                        }
                        else
                        {
                            if (!item.ApplicationDiscoveryURL.URL.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                            {
                                urls.Add((item.ApplicationDiscoveryURL, applicationDiscoveryItem.ControllerRoute));
                            }
                        }
                    }
                }
            }
            return GetHealthyUrls(urls);
        }
        protected List<string> GetHealthyUrls(List<(DomainModels.ApplicationDiscoveryURL, string)> urls)
        {
            List<string> urlList = new List<string>();
            if (urls != null && urls.Count > 0)
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow - Configuration.GetTimespanValue("DiscoveryURLHealthStatusTimeSpan", new TimeSpan(0, 5, 0));
                foreach ((DomainModels.ApplicationDiscoveryURL urlItem, string controllerRoute) in urls)
                {
                    //(string? currentHealthStatus, DateTimeOffset? lastStatusCheckDateTime)=await ApplicationDiscoveryRepository.GetCurrentHealthStatus(urlItem.ID);
                    if (string.Equals(urlItem.HealthStatus, "Healthy", StringComparison.OrdinalIgnoreCase) && urlItem.LastHealthStatusCheckDateTime > dateTimeOffset)
                    {
                        if (urlItem.Port != null && urlItem.Port > 0)
                        {
                            string formattedURL = $"{urlItem.URL}:{urlItem.Port}/{controllerRoute}";
                            if (!urlList.Contains(formattedURL))
                            {
                                urlList.Add(formattedURL);
                            }
                        }
                        else
                        {
                            string formattedURL = $"{urlItem.URL}/{controllerRoute}";
                            if (!urlList.Contains(formattedURL))
                            {
                                urlList.Add(formattedURL);
                            }
                        }
                    }
                }
            }
            return urlList;
        }

        public async Task<Model.ApplicationDiscoveryRoutes> GetRoutes(string friendlyName)
        {
            Model.ApplicationDiscoveryRoutes applicationDiscoveryRoutes = new Model.ApplicationDiscoveryRoutes();

            ApplicationDiscoveryItem applicationDiscoveryItem = await ApplicationDiscoveryRepository.GetDiscoveryItemByFriendlyName(friendlyName).ConfigureAwait(false);
            if (applicationDiscoveryItem != null)
            {
                foreach (ApplicationDiscoveryMethodApplicationDiscoveryItem applicationDiscoveryMethodApplicationDiscoveryItem in applicationDiscoveryItem.ApplicationDiscoveryMethodsApplicationDiscoveryItems)
                {
                    applicationDiscoveryRoutes.Routes.Add(new Model.ApplicationDiscoveryRouteItem(
                        applicationDiscoveryMethodApplicationDiscoveryItem.ApplicationDiscoveryMethod.HttpMethod,
                        applicationDiscoveryMethodApplicationDiscoveryItem.ApplicationDiscoveryMethod.Template,
                        applicationDiscoveryMethodApplicationDiscoveryItem.ApplicationDiscoveryMethod.MethodName
                    ));
                }
            }
            return applicationDiscoveryRoutes;
        }
        public async Task<ApplicationDiscoveryItem> AddDiscoveryRecords(ApplicationDiscoveryEntry applicationDiscoveryEntry)
        {
            ApplicationDiscoveryInfo info = await ApplicationDiscoveryRepository.CreateOrFind(applicationDiscoveryEntry.FriendlyName, applicationDiscoveryEntry.ControllerName, applicationDiscoveryEntry.ControllerRoute, applicationDiscoveryEntry.ApplicationDiscoveryURLs, applicationDiscoveryEntry.ApplicationDiscoveryMethods).ConfigureAwait(false);
            await ApplicationDiscoveryRepository.Save(info, applicationDiscoveryEntry).ConfigureAwait(false);
            return info.ApplicationDiscoveryItem;
        }

        public async Task<List<ApplicationDiscoveryHealthStatusResult>> GetAllApplicationHealthStatus()
        {
            List<ApplicationDiscoveryHealthStatusResult> results = new List<ApplicationDiscoveryHealthStatusResult>();

            List<ApplicationRegistryItem> applicationRegistryItems = await new ApplicationRegistryRepository(Configuration).GetAllRegistryItems().ConfigureAwait(false);

            foreach (ApplicationRegistryItem item in applicationRegistryItems)
            {
                Model.ApplicationDiscoveryHealthStatusResult result = AddHealthStatusItem(item);
                results.Add(result);
            }

            return results;
        }

        public async Task<List<ApplicationDiscoveryHealthStatusResult>> GetApplicationHealthStatusByLatest()
        {
            List<ApplicationRegistryItem> applicationRegistryItems = await new ApplicationRegistryRepository(Configuration).GetAllRegistryItems().ConfigureAwait(false);
            List<ApplicationDiscoveryHealthStatusResult> applicationHealthStatusList = new List<ApplicationDiscoveryHealthStatusResult>();

            foreach (ApplicationRegistryItem item in applicationRegistryItems)
            {
                ApplicationDiscoveryHealthStatusResult applicationHealthItem = new ApplicationDiscoveryHealthStatusResult()
                {
                    ApplicationName = item.ApplicationName,
                    FirstInstallDateTime = item.FirstInstallDateTime.UtcDateTime
                };
                applicationHealthItem.Versions.Add(GetLatestApplicationVersion(item));
                applicationHealthStatusList.Add(applicationHealthItem);
            }
            return applicationHealthStatusList;
        }

        protected ApplicationDiscoveryHealthStatusResult AddHealthStatusItem(ApplicationRegistryItem item)
        {
            ApplicationDiscoveryHealthStatusResult result = new ApplicationDiscoveryHealthStatusResult
            {
                ApplicationName = item.ApplicationName,
                FirstInstallDateTime = item.FirstInstallDateTime.UtcDateTime
            };

            foreach (ApplicationRegistryItemApplicationRegistryVersion versionItem in item.ApplicationRegistryItemApplicationRegistryVersions)
            {
                result.Versions.Add(AddHealthCheckVersion(versionItem.ApplicationRegistryVersion));
            }

            return result;
        }
        protected HealthCheckVersions AddHealthCheckVersion(ApplicationRegistryVersion versionItem)
        {
            HealthCheckVersions version = new HealthCheckVersions()
            {
                ApplicationVersion = versionItem.ApplicationVersion,
                FirstInstallDateTime = versionItem.FirstInstallDateTime
            };

            foreach (ApplicationRegistryVersionApplicationRegistryInstance instanceItem in versionItem.ApplicationRegistryVersionApplicationRegistryInstances)
            {
                version.Instances.Add(AddHealthCheckInstance(instanceItem));
            }

            foreach (ApplicationRegistryVersionApplicationDiscoveryItem discoveryItem in versionItem.ApplicationRegistryVersionApplicationDiscoveryItems)
            {
                version.FriendlyNames.Add(discoveryItem.ApplicationDiscoveryItem.FriendlyName);
                version.URLs = AddHealthCheckVersion(discoveryItem.ApplicationDiscoveryItem);
            }

            return version;
        }
        protected List<HealthCheckURLs> AddHealthCheckVersion(ApplicationDiscoveryItem applicationDiscoveryItem)
        {
            List<HealthCheckURLs> list = new List<HealthCheckURLs>();
            foreach (ApplicationDiscoveryURLApplicationDiscoveryItem urlItem in applicationDiscoveryItem.ApplicationDiscoveryURLApplicationDiscoveryItems)
            {
                list.Add(new HealthCheckURLs()
                {
                    URL = urlItem.ApplicationDiscoveryURL.URL,
                    HealthStatus = urlItem.ApplicationDiscoveryURL.HealthStatus,
                    LastHealthStatusDateTime = urlItem.ApplicationDiscoveryURL.LastHealthStatusCheckDateTime.GetValueOrDefault().UtcDateTime
                });
            }
            return list;
        }
        protected HealthCheckInstances AddHealthCheckInstance(ApplicationRegistryVersionApplicationRegistryInstance instanceItem)
        {
            HealthCheckInstances instance = new HealthCheckInstances()
            {
                ApplicationPath = instanceItem.ApplicationRegistryInstance.ApplicationPath,
                InstallDate = instanceItem.ApplicationRegistryInstance.InstallDateTime.UtcDateTime,
                IsActive = instanceItem.ApplicationRegistryInstance.IsActive,
                LastHeartbeatDateTime = instanceItem.ApplicationRegistryInstance.LastHeartbeatDateTime.GetValueOrDefault().UtcDateTime,
                LastStartDateTime = instanceItem.ApplicationRegistryInstance.LastStartDateTime.UtcDateTime,
                MachineName = instanceItem.ApplicationRegistryInstance.MachineName,
                NumberHeartbeats = instanceItem.ApplicationRegistryInstance.NumberHeartbeats,
            };
            return instance;
        }

        //protected List<string> GetUniqueDiscoveryURLs(List<string> urls)
        //{
        //    List<string> uniqueURLs = new List<string>();
        //    foreach (string url in urls)
        //    {
        //        if (!uniqueURLs.Contains(url))
        //        {
        //            uniqueURLs.Add(url);
        //        }
        //    }
        //    return uniqueURLs;
        //}
        public async Task<List<ApplicationDiscoveryHealthStatusResult>> GetApplicationHealthStatusByFriendlyName(string friendlyName)
        {
            ApplicationDiscoveryItem applicationDiscoveryItem = await ApplicationDiscoveryRepository.GetDiscoveryItemByFriendlyName(friendlyName).ConfigureAwait(false);
            List<ApplicationDiscoveryHealthStatusResult> results = new List<ApplicationDiscoveryHealthStatusResult>();

            if (applicationDiscoveryItem == null)
            {
                return results;
            }
            foreach (ApplicationRegistryVersionApplicationDiscoveryItem applicationRegistryVersionApplicationDiscoveryItem in applicationDiscoveryItem.ApplicationRegistryVersionsApplicationDiscoveryItems)
            {
                foreach (ApplicationRegistryItemApplicationRegistryVersion applicationRegistryItemApplicationRegistryVersion in applicationRegistryVersionApplicationDiscoveryItem.ApplicationRegistryVersion.ApplicationRegistryItemApplicationRegistryVersions)
                {
                    Model.ApplicationDiscoveryHealthStatusResult result = AddHealthStatusItem(applicationRegistryItemApplicationRegistryVersion.ApplicationRegistryItem);
                    results.Add(result);
                    return results;
                }
            }

            return results;
        }

        public HealthCheckVersions GetLatestApplicationVersion(ApplicationRegistryItem applicationRegistryItem)
        {
            ApplicationRegistryVersion? version = applicationRegistryItem.ApplicationRegistryItemApplicationRegistryVersions.OrderByDescending(p => p.ApplicationRegistryVersion.FirstInstallDateTime).Select(x => x.ApplicationRegistryVersion).FirstOrDefault();
            ApplicationRegistryInstance instance = version.ApplicationRegistryVersionApplicationRegistryInstances.OrderByDescending(p => p.ApplicationRegistryInstance.LastStartDateTime).Select(x => x.ApplicationRegistryInstance).FirstOrDefault();
            HealthCheckVersions healthCheckVersions = new HealthCheckVersions()
            {
                FirstInstallDateTime = version.FirstInstallDateTime,
                ApplicationVersion = version.ApplicationVersion,
                Instances = GetLatestRegistryInstance(instance),
                FriendlyNames = version.ApplicationRegistryVersionApplicationDiscoveryItems.Select(x => x.ApplicationDiscoveryItem.FriendlyName).Distinct().ToList(),
                URLs = GetUniqueDiscoveryURL(version.ApplicationRegistryVersionApplicationDiscoveryItems.Select(x => x.ApplicationDiscoveryItem).ToList())
            };
            return healthCheckVersions;
        }

        private List<HealthCheckInstances> GetLatestRegistryInstance(ApplicationRegistryInstance instance)
        {
            List<HealthCheckInstances> healthCheckInstances = new List<HealthCheckInstances>
            {
                AddHealthCheckInstances(instance)
            };
            return healthCheckInstances;
        }

        private HealthCheckInstances AddHealthCheckInstances(ApplicationRegistryInstance instance)
        {
            HealthCheckInstances healthCheckInstances = new HealthCheckInstances()
            {
                ApplicationPath = instance.ApplicationPath,
                InstallDate = instance.InstallDateTime.UtcDateTime,
                IsActive = instance.IsActive,
                LastHeartbeatDateTime = instance.LastHeartbeatDateTime.GetValueOrDefault().UtcDateTime,
                LastStartDateTime = instance.LastStartDateTime.UtcDateTime,
                MachineName = instance.MachineName,
                NumberHeartbeats = instance.NumberHeartbeats
            };
            return healthCheckInstances;
        }

        private List<HealthCheckURLs> GetUniqueDiscoveryURL(List<ApplicationDiscoveryItem> applicationDiscoveryItems)
        {
            Dictionary<string, DomainModels.ApplicationDiscoveryURL> urls = new Dictionary<string, DomainModels.ApplicationDiscoveryURL>();
            List<HealthCheckURLs> healthCheckURLs = new List<HealthCheckURLs>();

            foreach (ApplicationDiscoveryItem item in applicationDiscoveryItems)
            {
                foreach (ApplicationDiscoveryURLApplicationDiscoveryItem urlItem in item.ApplicationDiscoveryURLApplicationDiscoveryItems)
                {
                    string urlPort = urlItem.ApplicationDiscoveryURL.URL + urlItem.ApplicationDiscoveryURL.Port;
                    if (!urls.ContainsKey(urlPort))
                    {
                        urls.Add(urlPort, urlItem.ApplicationDiscoveryURL);
                    }
                }
            }
            foreach (KeyValuePair<string, DomainModels.ApplicationDiscoveryURL> url in urls)
            {
                healthCheckURLs.Add(new HealthCheckURLs()
                {
                    URL = url.Value.URL,
                    HealthStatus = url.Value.HealthStatus,
                    LastHealthStatusDateTime = url.Value.LastHealthStatusCheckDateTime.GetValueOrDefault().UtcDateTime
                });
            }
            return healthCheckURLs;
        }

        public async Task UpdateURL(DomainModels.ApplicationDiscoveryURL urlItem)
        {
            await ApplicationDiscoveryRepository.Save(urlItem).ConfigureAwait(false);
        }

        public async Task<List<DomainModels.ApplicationDiscoveryURL>> GetHealthyOrUnhealthyURLs(bool getHealthyURLs, DateTimeOffset outdatedURLStatusCheck, bool joinWithInstance = true, bool instanceActive = true)
        {
            return await ApplicationDiscoveryRepository.GetURLs(getHealthyURLs, outdatedURLStatusCheck).ConfigureAwait(false);
        }
    }
}
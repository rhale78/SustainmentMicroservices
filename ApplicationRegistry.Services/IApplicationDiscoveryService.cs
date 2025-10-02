using ApplicationRegistry.DomainModels;
using ApplicationRegistry.Model;

namespace ApplicationRegistry.Services
{
    public interface IApplicationDiscoveryService
    {
        Task<ApplicationDiscoveryItem> AddDiscoveryRecords(ApplicationDiscoveryEntry applicationDiscoveryEntry);
        Task<List<ApplicationDiscoveryHealthStatusResult>> GetAllApplicationHealthStatus();
        Task<List<ApplicationDiscoveryHealthStatusResult>> GetApplicationHealthStatusByFriendlyName(string friendlyName);
        Task<List<ApplicationDiscoveryHealthStatusResult>> GetApplicationHealthStatusByLatest();
        Task<List<DomainModels.ApplicationDiscoveryURL>> GetHealthyOrUnhealthyURLs(bool getHealthyURLs, DateTimeOffset outdatedURLStatusCheck, bool joinWithInstance = true, bool instanceActive = true);
        HealthCheckVersions GetLatestApplicationVersion(ApplicationRegistryItem applicationRegistryItem);
        Task<ApplicationDiscoveryRoutes> GetRoutes(string friendlyName);
        Task<List<string>> GetURLsForFriendlyName(string friendlyName, bool limitToHttps);
        Task UpdateURL(DomainModels.ApplicationDiscoveryURL urlItem);
        Task RegisterSelf(ApplicationRegistryResult applicationRegistryResult);
        Task SetURLsDown(int iD);
    }
}
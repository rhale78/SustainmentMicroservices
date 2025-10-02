using ApplicationRegistry.DomainModels;
using ApplicationRegistry.Model;

namespace ApplicationRegistry.Interfaces
{
    public interface IApplicationDiscoveryRepository
    {
        Task<ApplicationDiscoveryInfo> CreateOrFind(string friendlyName, string controllerName, string controllerRoute, List<Model.ApplicationDiscoveryURL> urls, List<Model.ApplicationDiscoveryMethod> methods);
        //Task<(string? currentHealthStatus, DateTimeOffset? lastStatusCheckDateTime)> GetCurrentHealthStatus(int iD);
        Task<ApplicationDiscoveryItem> GetDiscoveryItemByFriendlyName(string friendlyName);
        //Task Purge();
        //Task<ApplicationDiscoveryInfo> Save(ApplicationDiscoveryItem applicationDiscoveryItem);
        Task<ApplicationDiscoveryInfo> Save(Model.ApplicationDiscoveryInfo applicationDiscoveryInfo, Model.ApplicationDiscoveryEntry applicationDiscoveryEntry);
        Task Save(DomainModels.ApplicationDiscoveryURL urlItem);
        Task SetURLsDown(int applicationInstanceID);
        Task<List<DomainModels.ApplicationDiscoveryURL>> GetURLs(bool healthy, DateTimeOffset withinTimeframeStart, bool joinWithInstance = true, bool instanceActive = true);
    }
}

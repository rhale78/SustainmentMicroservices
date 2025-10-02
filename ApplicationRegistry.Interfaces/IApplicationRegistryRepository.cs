using ApplicationRegistry.DomainModels;
using ApplicationRegistry.Model;

namespace ApplicationRegistry.Interfaces
{
    public interface IApplicationRegistryRepository
    {
        Task<ApplicationRegistryInfo> CreateOrFind(string applicationName, string applicationVersion, string applicationPath, string machineName, string applicationHash);
        Task<ApplicationRegistryApplicationIDs> Save(ApplicationRegistryInfo applicationRegistryInfo);
        Task<ApplicationRegistryInstance> Save(ApplicationRegistryInstance applicationRegistryInstance);
        Task<ApplicationRegistryInstance> GetApplicationRegistryInstanceByID(int applicationInstanceID, bool activeOnly = false);
        void UpgradeAndPurgeDatabase();
        //Task Purge();
        //Task UpgradeDatabase();
    }
}

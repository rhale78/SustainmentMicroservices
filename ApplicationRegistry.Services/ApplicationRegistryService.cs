using ApplicationRegistry.Common;
using ApplicationRegistry.DomainModels;
using ApplicationRegistry.Interfaces;
using ApplicationRegistry.Model;
using ApplicationRegistry.Repository;
using Log;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Services
{
    public class ApplicationRegistryService : IApplicationRegistryService
    {
        public IConfiguration Configuration { get; }
        public IApplicationRegistryRepository ApplicationRegistryRepository { get; }
        public LogInstance Log { get; set; } = LogInstance.CreateLog();

        protected IApplicationDiscoveryService ApplicationDiscoveryService { get; set; }
        public ApplicationRegistryService(IConfiguration configuration, IApplicationDiscoveryService applicationDiscoveryService)
        {
            Configuration = configuration;
            ApplicationRegistryRepository = new ApplicationRegistryRepository(Configuration);
            ApplicationDiscoveryService = applicationDiscoveryService ?? new ApplicationDiscoveryService(Configuration);
            RegisterSelf();
        }

        public async Task<ApplicationRegistryResult> Register(ApplicationRegistryEntry applicationRegistryEntry)
        {
            ApplicationRegistryInfo info = await ApplicationRegistryRepository.CreateOrFind(applicationRegistryEntry.ApplicationName, applicationRegistryEntry.ApplicationVersion, applicationRegistryEntry.ApplicationPath, applicationRegistryEntry.MachineName, applicationRegistryEntry.ApplicationHash).ConfigureAwait(false);
            bool needsSave = false;
            if (info.NewApplicationItem)
            {
                if (Configuration.GetBoolValueWithDefault("AllowNewApplicationRegistration", true))
                {
                    Log.LogInformation("Registering new application {applicationName}", applicationRegistryEntry.ApplicationName);
                    needsSave = true;
                }
                else
                {
                    string message = "Cannot register application - AllowNewApplicationRegistration is disabled";
                    Log.LogError(message);
                    throw new Exception(message);
                }
            }
            if (info.NewApplicationInstance)
            {
                if (Configuration.GetBoolValueWithDefault("AllowNewApplicationInstance", true))
                {
                    Log.LogInformation("Registering new application instance {applicationPath}", applicationRegistryEntry.ApplicationPath);
                    needsSave = true;
                }
                else
                {
                    string message = "Cannot register application - AllowNewApplicationInstance is disabled";
                    Log.LogError(message);
                    throw new Exception(message);
                }
            }
            if (info.NewApplicationVersion)
            {
                if (Configuration.GetBoolValueWithDefault("AllowNewApplicationVersion", true))
                {
                    Log.LogInformation("Registering new application version {applicationVersion}", applicationRegistryEntry.ApplicationVersion);
                    needsSave = true;
                }
                else
                {
                    string message = "Cannot register application - AllowNewApplicationVersion is disabled";
                    Log.LogError(message);
                    throw new Exception(message);
                }
            }
            if (needsSave)
            {
                ApplicationRegistryApplicationIDs ids = await ApplicationRegistryRepository.Save(info).ConfigureAwait(false);

                return new ApplicationRegistryResult()
                {
                    ApplicationID = ids.ApplicationRegistryItemID,
                    ApplicationInstanceID = ids.ApplicationRegistryInstanceID,
                    ApplicationVersionID = ids.ApplicationRegistryVersionID,
                    UpgradeOrCreate = needsSave
                };
            }

            return new ApplicationRegistryResult()
            {
                ApplicationID = info.ApplicationRegistryItem.ID,
                ApplicationInstanceID = info.ApplicationRegistryInstance.ID,
                ApplicationVersionID = info.ApplicationRegistryVersion.ID,
                UpgradeOrCreate = needsSave
            };
        }

        public async Task<VerificationStatusTypeEnum> VerifyApplicationModel(VerifyApplicationModel verifyApplicationModel)
        {
            DomainModels.ApplicationRegistryInstance registryInstance = await ApplicationRegistryRepository.GetApplicationRegistryInstanceByID(verifyApplicationModel.ApplicationInstanceID, true).ConfigureAwait(false);
            if (registryInstance == null)
            {
                registryInstance = await ApplicationRegistryRepository.GetApplicationRegistryInstanceByID(verifyApplicationModel.ApplicationInstanceID, false).ConfigureAwait(false);
                return registryInstance == null ? VerificationStatusTypeEnum.UnknownApplication : VerificationStatusTypeEnum.NotActiveApplication;
            }

            //RSH 2/1/24 - should probably verify app name, etc incl url/port - skipping for now

            //RSH 2/1/24 - find by version+hash
            foreach (DomainModels.ApplicationRegistryVersionApplicationRegistryInstance applicationRegistryVersionApplicationRegistryInstance in registryInstance.ApplicationRegistryVersionApplicationRegistryInstances)
            {
                if (string.Equals(verifyApplicationModel.ApplicationVersion, applicationRegistryVersionApplicationRegistryInstance.ApplicationRegistryVersion?.ApplicationVersion, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(verifyApplicationModel.Hash, applicationRegistryVersionApplicationRegistryInstance.ApplicationRegistryVersion?.ApplicationHash))
                    {
                        return VerificationStatusTypeEnum.AccessAllowed;
                    }
                }
            }

            return VerificationStatusTypeEnum.InvalidApplicationData;
        }

        public async Task<ApplicationRegistryHierarchyForApplicationInstanceResult> GetApplicationRegistryHierarchyInfo(int applicationInstanceID)
        {
            ApplicationRegistryHierarchyForApplicationInstanceResult result = new ApplicationRegistryHierarchyForApplicationInstanceResult();

            ApplicationRegistryInstance? instance = await ApplicationRegistryRepository.GetApplicationRegistryInstanceByID(applicationInstanceID).ConfigureAwait(false);
            if (instance == null)
            {
                return null;
            }

            ApplicationRegistryVersion? applicationRegistryVersion = instance.ApplicationRegistryVersionApplicationRegistryInstances.MaxBy(p => p.ApplicationRegistryVersion.FirstInstallDateTime)?.ApplicationRegistryVersion;
            result.CurrentInstanceID = instance.ID;
            result.CurrentVersionID = applicationRegistryVersion.ID;    //RSH 2/1/24 - this is not necessarily correct - this is just the latest new install (ie downgrade may cause this to be wrong)
            result.RegistryID = applicationRegistryVersion.ApplicationRegistryItemApplicationRegistryVersions.FirstOrDefault().ApplicationRegistryItemID;

            //RSH 2/1/24 - no previous instances since we search before create
            result.PreviousVersionIDs = instance.ApplicationRegistryVersionApplicationRegistryInstances.Where(p => p.ApplicationRegistryVersionID != result.CurrentVersionID).OrderByDescending(p => p.ApplicationRegistryVersion.FirstInstallDateTime).Select(p => p.ApplicationRegistryVersionID);
            return result;
        }

        public async Task<ApplicationRegistryResult> RegisterSelf()
        {
            //ApplicationDiscoveryService applicationDiscoveryService = new ApplicationDiscoveryService(Configuration);

            ApplicationRegistryRepository.UpgradeAndPurgeDatabase();

            ApplicationRegistryEntry registryEntry = await Helpers.GetRegistryEntry(Log, Configuration).ConfigureAwait(false);
            ApplicationRegistryResult result = await Register(registryEntry).ConfigureAwait(false);

            await ApplicationDiscoveryService.RegisterSelf(result).ConfigureAwait(false);
            InstanceIsActive isActiveInstance = new InstanceIsActive() { ApplicationInstanceID = result.ApplicationInstanceID, Starting = true };
            await SetApplicationActiveFlag(isActiveInstance).ConfigureAwait(false);
            return result;
        }

        public async Task SetApplicationActiveFlag(InstanceIsActive isActiveInstance)
        {
            if (isActiveInstance != null)
            {
                if (isActiveInstance.ApplicationInstanceID > 0)
                {
                    ApplicationRegistryInstance instance = await ApplicationRegistryRepository.GetApplicationRegistryInstanceByID(isActiveInstance.ApplicationInstanceID).ConfigureAwait(false);

                    if (instance != null)
                    {
                        if (isActiveInstance.Starting)
                        {
                            instance.IsActive = true;
                            instance.LastStartDateTime = DateTime.UtcNow;
                            await ApplicationRegistryRepository.Save(instance).ConfigureAwait(false);
                        }
                        else
                        {
                            instance.IsActive = false;
                            await ApplicationRegistryRepository.Save(instance).ConfigureAwait(false);

                            //ApplicationDiscoveryService applicationDiscoveryService = new ApplicationDiscoveryService(Configuration);
                            await ApplicationDiscoveryService.SetURLsDown(instance.ID).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        public async Task<ApplicationActiveResult> IsActive(ApplicationActiveRequest applicationActiveRequest)
        {
            ApplicationActiveResult result = new ApplicationActiveResult();

            ApplicationRegistryInstance instance = await ApplicationRegistryRepository.GetApplicationRegistryInstanceByID(applicationActiveRequest.ApplicationInstanceID).ConfigureAwait(false);
            if (instance == null)
            {
                return result;
            }

            //RSH 2/1/24 - this is a correction/change from R4 - it just checked that the friendly name existed at all, not tied to the instance nor the health status
            result.IsAvailable = true;
            bool valid = true;
            bool found = false;
            foreach (ApplicationRegistryInstanceApplicationDiscoveryURL url in instance.ApplicationRegistryInstancesApplicationDiscoveryURLs)
            {
                if (string.Equals(url.ApplicationDiscoveryURL.HealthStatus, "Healthy", StringComparison.OrdinalIgnoreCase))
                {
                    IEnumerable<ApplicationDiscoveryURLApplicationDiscoveryItem> matching = url.ApplicationDiscoveryURL.ApplicationDiscoveryURLApplicationDiscoveryItems.Where(p => string.Equals(p.ApplicationDiscoveryItem.FriendlyName, applicationActiveRequest.FriendlyName, StringComparison.OrdinalIgnoreCase));
                    if (matching != null && matching.Count() > 0)
                    {
                        valid &= true;
                        found = true;
                    }
                }
                else
                {
                    IEnumerable<ApplicationDiscoveryURLApplicationDiscoveryItem> matching = url.ApplicationDiscoveryURL.ApplicationDiscoveryURLApplicationDiscoveryItems.Where(p => string.Equals(p.ApplicationDiscoveryItem.FriendlyName, applicationActiveRequest.FriendlyName, StringComparison.OrdinalIgnoreCase));
                    if (matching != null && matching.Count() > 0)
                    {
                        valid &= false;
                        found = true;
                    }
                }
            }
            result.IsActive = found && valid;
            return result;
        }

        public async Task<WhoIsResponse> WhoIs(int applicationInstanceID)
        {
            ApplicationRegistryInstance applicationRegistryInstance = await ApplicationRegistryRepository.GetApplicationRegistryInstanceByID(applicationInstanceID).ConfigureAwait(false);
            if (applicationRegistryInstance == null)
            {
                throw new Exception($"Application instance {applicationInstanceID} not found");
            }
            if (applicationRegistryInstance.ApplicationRegistryVersionApplicationRegistryInstances == null)
            {
                throw new Exception($"Application instance {applicationInstanceID} not found");
            }
            ApplicationRegistryVersionApplicationRegistryInstance registryVersion = applicationRegistryInstance.ApplicationRegistryVersionApplicationRegistryInstances.FirstOrDefault();
            if (registryVersion == null)
            {
                throw new Exception($"Application instance {applicationInstanceID} not found");
            }
            if (registryVersion.ApplicationRegistryVersion == null)
            {
                throw new Exception($"Application instance {applicationInstanceID} not found");
            }
            if (registryVersion.ApplicationRegistryVersion.ApplicationRegistryItemApplicationRegistryVersions == null)
            {
                throw new Exception($"Application instance {applicationInstanceID} not found");
            }
            ApplicationRegistryItem item = registryVersion.ApplicationRegistryVersion.ApplicationRegistryItemApplicationRegistryVersions.FirstOrDefault().ApplicationRegistryItem;
            return item == null
                ? throw new Exception($"Application instance {applicationInstanceID} not found")
                : new WhoIsResponse() { ApplicationName = item.ApplicationName };
        }

        public async Task<ApplicationInstanceLocationResponse> GetInstanceLocation(int applicationInstanceID)
        {
            ApplicationRegistryInstance applicationRegistryInstance = await ApplicationRegistryRepository.GetApplicationRegistryInstanceByID(applicationInstanceID).ConfigureAwait(false);
            return applicationRegistryInstance == null
                ? throw new Exception($"Application instance {applicationInstanceID} not found")
                : new ApplicationInstanceLocationResponse() { Path = applicationRegistryInstance.ApplicationPath, ServerName = applicationRegistryInstance.MachineName };
        }

        public async Task IncrementRegistryInstanceHeartbeat(int applicationInstanceID)
        {
            ApplicationRegistryInstance applicationRegistryInstance = await ApplicationRegistryRepository.GetApplicationRegistryInstanceByID(applicationInstanceID).ConfigureAwait(false);
            if (applicationRegistryInstance == null)
            {
                throw new Exception($"Application instance {applicationInstanceID} not found");
            }

            applicationRegistryInstance.NumberHeartbeats += 1;
            applicationRegistryInstance.LastHeartbeatDateTime = DateTime.UtcNow;
            applicationRegistryInstance.IsActive = true;
            await ApplicationRegistryRepository.Save(applicationRegistryInstance).ConfigureAwait(false);
        }

    }
}
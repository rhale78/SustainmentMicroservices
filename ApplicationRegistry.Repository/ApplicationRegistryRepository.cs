using Common.DAL;
using ApplicationRegistry.DomainModels;
using ApplicationRegistry.Interfaces;
using ApplicationRegistry.Model;
using Microsoft.Extensions.Configuration;
using Common.Repository;

namespace ApplicationRegistry.Repository
{
    public class ApplicationRegistryRepository : RootRepository<ApplicationRegistryRepository>, IApplicationRegistryRepository
    {
        internal ApplicationRegistryInstanceRepository ApplicationRegistryInstanceRepository { get; set; }
        internal ApplicationRegistryItemRepository ApplicationRegistryItemRepository { get; set; }
        internal ApplicationRegistryVersionRepository ApplicationRegistryVersionRepository { get; set; }

        internal ApplicationRegistryVersionApplicationRegistryInstanceRepository ApplicationRegistryVersionApplicationRegistryInstanceRepository { get; set; }
        internal ApplicationRegistryItemApplicationRegistryVersionRepository ApplicationRegistryItemApplicationRegistryVersionRepository { get; set; }

        internal ApplicationDiscoveryRepository ApplicationDiscoveryRepository { get; set; }

        public ApplicationRegistryRepository(IConfiguration configuration) : base(configuration)
        {
            ApplicationRegistryInstanceRepository = new ApplicationRegistryInstanceRepository(Configuration);
            ApplicationRegistryItemRepository = new ApplicationRegistryItemRepository(Configuration);
            ApplicationRegistryVersionRepository = new ApplicationRegistryVersionRepository(Configuration);

            ApplicationRegistryItemApplicationRegistryVersionRepository = new ApplicationRegistryItemApplicationRegistryVersionRepository(Configuration);
            ApplicationRegistryVersionApplicationRegistryInstanceRepository = new ApplicationRegistryVersionApplicationRegistryInstanceRepository(Configuration);

            ApplicationDiscoveryRepository = new ApplicationDiscoveryRepository(Configuration);
        }

        public void UpgradeAndPurgeDatabase()
        {
            if (!IsRepositoryReady)
            {
                lock (LockObject)
                {
                    if (!IsRepositoryReady)
                    {
                        try
                        {
                            UpgradeDatabase().Wait();
                            Purge().Wait();
                        }
                        finally
                        {
                            IsRepositoryReady = true;
                        }
                    }
                }
            }
        }

        internal async Task UpgradeDatabase()
        {
            if (Configuration.GetBoolValueWithDefault("UpgradeDatabase", false))
            {
                await DropTables().ConfigureAwait(false);
                await CreateTables().ConfigureAwait(false);
            }
        }

        public override async Task DropTables()
        {
            await ApplicationRegistryItemApplicationRegistryVersionRepository.DropTable().ConfigureAwait(false);
            await ApplicationRegistryVersionApplicationRegistryInstanceRepository.DropTable().ConfigureAwait(false);

            await ApplicationDiscoveryRepository.DropTables().ConfigureAwait(false);

            await ApplicationRegistryInstanceRepository.DropTable().ConfigureAwait(false);
            await ApplicationRegistryItemRepository.DropTable().ConfigureAwait(false);
            await ApplicationRegistryVersionRepository.DropTable().ConfigureAwait(false);
        }

        public override async Task CreateTables()
        {
            await ApplicationRegistryInstanceRepository.CreateTable().ConfigureAwait(false);
            await ApplicationRegistryItemRepository.CreateTable().ConfigureAwait(false);
            await ApplicationRegistryVersionRepository.CreateTable().ConfigureAwait(false);

            await ApplicationDiscoveryRepository.CreateTables().ConfigureAwait(false);

            await ApplicationRegistryItemApplicationRegistryVersionRepository.CreateTable().ConfigureAwait(false);
            await ApplicationRegistryVersionApplicationRegistryInstanceRepository.CreateTable().ConfigureAwait(false);
        }

        public override async Task Purge()
        {
            if (Configuration.GetBoolValueWithDefault("PurgeRegistry", false))
            {
                await ApplicationRegistryItemApplicationRegistryVersionRepository.Purge().ConfigureAwait(false);
                await ApplicationRegistryVersionApplicationRegistryInstanceRepository.Purge().ConfigureAwait(false);

                await ApplicationDiscoveryRepository.Purge().ConfigureAwait(false);

                await ApplicationRegistryInstanceRepository.Purge().ConfigureAwait(false);
                await ApplicationRegistryItemRepository.Purge().ConfigureAwait(false);
                await ApplicationRegistryVersionRepository.Purge().ConfigureAwait(false);
            }
        }

        public async Task<ApplicationRegistryInfo> CreateOrFind(string applicationName, string applicationVersion, string applicationPath, string machineName, string applicationHash)
        {
            await WaitForRepositoryToBeReady().ConfigureAwait(false);
            ApplicationRegistryInfo applicationRegistryInfo = new ApplicationRegistryInfo();

            ApplicationRegistryItem applicationRegistryItem = await ApplicationRegistryItemRepository.CreateOrFind(applicationName).ConfigureAwait(false);
            applicationRegistryInfo.NewApplicationItem = (applicationRegistryItem.ID == 0);
            applicationRegistryInfo.ApplicationRegistryItem = applicationRegistryItem;

            ApplicationRegistryVersion applicationRegistryVersion = await ApplicationRegistryVersionRepository.CreateOrFind(applicationVersion, applicationHash).ConfigureAwait(false);
            applicationRegistryInfo.NewApplicationVersion = (applicationRegistryVersion.ID == 0);
            applicationRegistryInfo.ApplicationRegistryVersion = applicationRegistryVersion;

            ApplicationRegistryInstance applicationRegistryInstance = await ApplicationRegistryInstanceRepository.CreateOrFind(applicationPath, machineName).ConfigureAwait(false);
            applicationRegistryInfo.NewApplicationInstance = (applicationRegistryInstance.ID == 0);
            applicationRegistryInfo.ApplicationRegistryInstance = applicationRegistryInstance;

            return applicationRegistryInfo;
        }

        public async Task<ApplicationRegistryInstance> GetApplicationRegistryInstanceByID(int applicationInstanceID, bool activeOnly = false)
        {
            await WaitForRepositoryToBeReady().ConfigureAwait(false);
            ApplicationRegistryInstance applicationRegistryInstance = await ApplicationRegistryInstanceRepository.GetByID(applicationInstanceID).ConfigureAwait(false);
            return applicationRegistryInstance == null
                ? applicationRegistryInstance
                : activeOnly && !applicationRegistryInstance.IsActive ? applicationRegistryInstance : applicationRegistryInstance;
        }

        public async Task<ApplicationRegistryApplicationIDs> Save(ApplicationRegistryInfo applicationRegistryInfo)
        {
            await WaitForRepositoryToBeReady().ConfigureAwait(false);
            CycleDetector cycleDetector = new CycleDetector();

            ApplicationRegistryApplicationIDs applicationRegistryApplicationIDs = new ApplicationRegistryApplicationIDs();

            AddRegistryItemRegistryVersionJoin(applicationRegistryInfo);
            AddRegistryInstanceRegistryVersionJoin(applicationRegistryInfo);

            applicationRegistryInfo.ApplicationRegistryItem = await ApplicationRegistryItemRepository.Save(applicationRegistryInfo.ApplicationRegistryItem, cycleDetector).ConfigureAwait(false);
            applicationRegistryApplicationIDs.ApplicationRegistryItemID = applicationRegistryInfo.ApplicationRegistryItem.ID;

            applicationRegistryInfo.ApplicationRegistryVersion = await ApplicationRegistryVersionRepository.Save(applicationRegistryInfo.ApplicationRegistryVersion, cycleDetector).ConfigureAwait(false);
            applicationRegistryApplicationIDs.ApplicationRegistryVersionID = applicationRegistryInfo.ApplicationRegistryVersion.ID;

            applicationRegistryInfo.ApplicationRegistryInstance = await ApplicationRegistryInstanceRepository.Save(applicationRegistryInfo.ApplicationRegistryInstance, cycleDetector).ConfigureAwait(false);
            applicationRegistryApplicationIDs.ApplicationRegistryInstanceID = applicationRegistryInfo.ApplicationRegistryInstance.ID;

            return applicationRegistryApplicationIDs;
        }

        private void AddRegistryItemRegistryVersionJoin(ApplicationRegistryInfo applicationRegistryInfo)
        {
            ApplicationRegistryItemApplicationRegistryVersion registryItemRegistryVersion = new ApplicationRegistryItemApplicationRegistryVersion()
            {
                ApplicationRegistryItem = applicationRegistryInfo.ApplicationRegistryItem,
                ApplicationRegistryVersion = applicationRegistryInfo.ApplicationRegistryVersion
            };
            applicationRegistryInfo.ApplicationRegistryItem.ApplicationRegistryItemApplicationRegistryVersions.Clear(); //RSH 1/19/24 - does not clear DB rows
            applicationRegistryInfo.ApplicationRegistryItem.ApplicationRegistryItemApplicationRegistryVersions.Add(registryItemRegistryVersion);
            applicationRegistryInfo.ApplicationRegistryVersion.ApplicationRegistryItemApplicationRegistryVersions.Clear(); //RSH 1/19/24 - does not clear DB rows
            applicationRegistryInfo.ApplicationRegistryVersion.ApplicationRegistryItemApplicationRegistryVersions.Add(registryItemRegistryVersion);
        }

        private void AddRegistryInstanceRegistryVersionJoin(ApplicationRegistryInfo applicationRegistryInfo)
        {
            ApplicationRegistryVersionApplicationRegistryInstance registryVersionRegistryInstance = new ApplicationRegistryVersionApplicationRegistryInstance()
            {
                ApplicationRegistryInstance = applicationRegistryInfo.ApplicationRegistryInstance,
                ApplicationRegistryVersion = applicationRegistryInfo.ApplicationRegistryVersion
            };
            applicationRegistryInfo.ApplicationRegistryInstance.ApplicationRegistryVersionApplicationRegistryInstances.Clear(); //RSH 1/19/24 - does not clear DB rows
            applicationRegistryInfo.ApplicationRegistryInstance.ApplicationRegistryVersionApplicationRegistryInstances.Add(registryVersionRegistryInstance);
            applicationRegistryInfo.ApplicationRegistryVersion.ApplicationRegistryVersionApplicationRegistryInstances.Clear(); //RSH 1/19/24 - does not clear DB rows
            applicationRegistryInfo.ApplicationRegistryVersion.ApplicationRegistryVersionApplicationRegistryInstances.Add(registryVersionRegistryInstance);
        }

        public async Task<ApplicationRegistryInstance> Save(ApplicationRegistryInstance applicationRegistryInstance)
        {
            await WaitForRepositoryToBeReady().ConfigureAwait(false);
            if (applicationRegistryInstance.ID != 0)
            {
                CycleDetector cycleDetector = new CycleDetector();
                return await ApplicationRegistryInstanceRepository.Save(applicationRegistryInstance, cycleDetector).ConfigureAwait(false);
            }
            throw new NotImplementedException();
        }

        public async Task<List<ApplicationRegistryItem>> GetAllRegistryItems()
        {
            await WaitForRepositoryToBeReady().ConfigureAwait(false);
            return await ApplicationRegistryItemRepository.GetAll().ConfigureAwait(false);
        }

        internal async Task<ApplicationRegistryVersion> GetApplicationRegistryVersionByID(int applicationVersionID)
        {
            await WaitForRepositoryToBeReady().ConfigureAwait(false);
            return await ApplicationRegistryVersionRepository.GetByID(applicationVersionID).ConfigureAwait(false);
        }

    }
}
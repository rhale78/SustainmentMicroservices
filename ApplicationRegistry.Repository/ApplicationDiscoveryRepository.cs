using ApplicationRegistry.DomainModels;
using ApplicationRegistry.Interfaces;
using ApplicationRegistry.Model;
using Common.DAL;
using Common.Repository;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Repository
{
    public class ApplicationDiscoveryRepository : RootRepository<ApplicationDiscoveryRepository>, IApplicationDiscoveryRepository
    {
        internal ApplicationDiscoveryItemRepository ApplicationDiscoveryItemRepository { get; set; }
        internal ApplicationDiscoveryMethodRepository ApplicationDiscoveryMethodRepository { get; set; }
        internal ApplicationDiscoveryURLRepository ApplicationDiscoveryURLRepository { get; set; }

        internal ApplicationDiscoveryMethodApplicationDiscoveryItemRepository ApplicationDiscoveryMethodApplicationDiscoveryItemRepository { get; set; }
        internal ApplicationRegistryInstanceApplicationDiscoveryURLsRepository ApplicationRegistryInstanceApplicationDiscoveryURLsRepository { get; set; }
        internal ApplicationRegistryVersionApplicationDiscoveryItemRepository ApplicationRegistryVersionApplicationDiscoveryItemRepository { get; set; }
        internal ApplicationDiscoveryURLApplicationDiscoveryItemRepository ApplicationDiscoveryURLApplicationDiscoveryItemRepository { get; set; }

        public ApplicationDiscoveryRepository(IConfiguration configuration) : base(configuration)
        {
            ApplicationDiscoveryItemRepository = new ApplicationDiscoveryItemRepository(Configuration);
            ApplicationDiscoveryMethodRepository = new ApplicationDiscoveryMethodRepository(Configuration);
            ApplicationDiscoveryURLRepository = new ApplicationDiscoveryURLRepository(Configuration);

            ApplicationDiscoveryMethodApplicationDiscoveryItemRepository = new ApplicationDiscoveryMethodApplicationDiscoveryItemRepository(Configuration);
            ApplicationRegistryInstanceApplicationDiscoveryURLsRepository = new ApplicationRegistryInstanceApplicationDiscoveryURLsRepository(Configuration);
            ApplicationRegistryVersionApplicationDiscoveryItemRepository = new ApplicationRegistryVersionApplicationDiscoveryItemRepository(Configuration);
            ApplicationDiscoveryURLApplicationDiscoveryItemRepository = new ApplicationDiscoveryURLApplicationDiscoveryItemRepository(Configuration);
        }

        public override async Task Purge()
        {
            if (Configuration.GetBoolValueWithDefault("PurgeRegistry", false))
            {
                await ApplicationDiscoveryMethodApplicationDiscoveryItemRepository.Purge().ConfigureAwait(false);
                await ApplicationRegistryInstanceApplicationDiscoveryURLsRepository.Purge().ConfigureAwait(false);
                await ApplicationRegistryVersionApplicationDiscoveryItemRepository.Purge().ConfigureAwait(false);
                await ApplicationDiscoveryURLApplicationDiscoveryItemRepository.Purge().ConfigureAwait(false);

                await ApplicationDiscoveryItemRepository.Purge().ConfigureAwait(false);
                await ApplicationDiscoveryMethodRepository.Purge().ConfigureAwait(false);
                await ApplicationDiscoveryURLRepository.Purge().ConfigureAwait(false);
            }
        }
        public override async Task DropTables()
        {
            await ApplicationDiscoveryMethodApplicationDiscoveryItemRepository.DropTable().ConfigureAwait(false);
            await ApplicationRegistryInstanceApplicationDiscoveryURLsRepository.DropTable().ConfigureAwait(false);
            await ApplicationRegistryVersionApplicationDiscoveryItemRepository.DropTable().ConfigureAwait(false);
            await ApplicationDiscoveryURLApplicationDiscoveryItemRepository.DropTable().ConfigureAwait(false);

            await ApplicationDiscoveryItemRepository.DropTable().ConfigureAwait(false);
            await ApplicationDiscoveryMethodRepository.DropTable().ConfigureAwait(false);
            await ApplicationDiscoveryURLRepository.DropTable().ConfigureAwait(false);
        }
        
        public override async Task CreateTables()
        {
            await ApplicationDiscoveryItemRepository.CreateTable().ConfigureAwait(false);
            await ApplicationDiscoveryMethodRepository.CreateTable().ConfigureAwait(false);
            await ApplicationDiscoveryURLRepository.CreateTable().ConfigureAwait(false);

            await ApplicationDiscoveryMethodApplicationDiscoveryItemRepository.CreateTable().ConfigureAwait(false);
            await ApplicationRegistryInstanceApplicationDiscoveryURLsRepository.CreateTable().ConfigureAwait(false);
            await ApplicationRegistryVersionApplicationDiscoveryItemRepository.CreateTable().ConfigureAwait(false);
            await ApplicationDiscoveryURLApplicationDiscoveryItemRepository.CreateTable().ConfigureAwait(false);
        }


        public async Task<Model.ApplicationDiscoveryInfo> CreateOrFind(string friendlyName, string controllerName, string controllerRoute, List<Model.ApplicationDiscoveryURL> urls, List<Model.ApplicationDiscoveryMethod> methods)
        {
            await WaitForRepositoryToBeReady().ConfigureAwait(false);
            Model.ApplicationDiscoveryInfo applicationDiscoveryInfo = new Model.ApplicationDiscoveryInfo();

            foreach (Model.ApplicationDiscoveryURL url in urls)
            {
                DomainModels.ApplicationDiscoveryURL applicationDiscoveryURL = await ApplicationDiscoveryURLRepository.CreateOrFind(url.URL, url.Port.GetValueOrDefault()).ConfigureAwait(false);
                applicationDiscoveryInfo.ApplicationDiscoveryURLs.Add((applicationDiscoveryURL.ID == 0, applicationDiscoveryURL));
            }

            ApplicationDiscoveryItem applicationDiscoveryItem = await ApplicationDiscoveryItemRepository.CreateOrFind(friendlyName, controllerName, controllerRoute).ConfigureAwait(false);
            applicationDiscoveryInfo.NewApplicationDiscoveryItem = (applicationDiscoveryItem.ID == 0);
            applicationDiscoveryInfo.ApplicationDiscoveryItem = applicationDiscoveryItem;

            foreach (Model.ApplicationDiscoveryMethod method in methods)
            {
                DomainModels.ApplicationDiscoveryMethod applicationDiscoveryMethod = await ApplicationDiscoveryMethodRepository.CreateOrFind(method.HttpMethod, method.MethodName, method.Template).ConfigureAwait(false);
                applicationDiscoveryInfo.ApplicationDiscoveryMethods.Add((applicationDiscoveryMethod.ID == 0, applicationDiscoveryMethod));
            }

            return applicationDiscoveryInfo;
        }

        public async Task<ApplicationDiscoveryInfo> Save(Model.ApplicationDiscoveryInfo applicationDiscoveryInfo, ApplicationDiscoveryEntry applicationDiscoveryEntry)
        {
            await WaitForRepositoryToBeReady().ConfigureAwait(false);
            CycleDetector cycleDetector = new CycleDetector();

            await AddRegistryInstanceDiscoveryURLJoin(applicationDiscoveryInfo, applicationDiscoveryEntry.ApplicationInstanceID).ConfigureAwait(false);
            AddDiscoveryURLDiscoveryItemJoin(applicationDiscoveryInfo);
            AddDiscoveryMethodDiscoveryItemJoin(applicationDiscoveryInfo);

            await AddRegistryVersionDiscoveryItemJoin(applicationDiscoveryInfo, applicationDiscoveryEntry.ApplicationVersionID).ConfigureAwait(false);
            await ApplicationDiscoveryItemRepository.Save(applicationDiscoveryInfo.ApplicationDiscoveryItem, cycleDetector).ConfigureAwait(false);

            foreach ((_, DomainModels.ApplicationDiscoveryURL applicationDiscoveryURL) in applicationDiscoveryInfo.ApplicationDiscoveryURLs)
            {
                await ApplicationDiscoveryURLRepository.Save(applicationDiscoveryURL, cycleDetector).ConfigureAwait(false);
            }
            //await ApplicationDiscoveryItemRepository.Save(applicationDiscoveryInfo.ApplicationDiscoveryItem, cycleDetector);
            foreach ((_, DomainModels.ApplicationDiscoveryMethod applicationDiscoveryMethod) in applicationDiscoveryInfo.ApplicationDiscoveryMethods)
            {
                await ApplicationDiscoveryMethodRepository.Save(applicationDiscoveryMethod, cycleDetector).ConfigureAwait(false);
            }

            return applicationDiscoveryInfo;
        }

        private async Task AddRegistryInstanceDiscoveryURLJoin(Model.ApplicationDiscoveryInfo applicationDiscoveryInfo, int applicationInstanceID)
        {
            await WaitForRepositoryToBeReady().ConfigureAwait(false);
            ApplicationRegistryRepository applicationRegistryRepository = new ApplicationRegistryRepository(Configuration);
            DomainModels.ApplicationRegistryInstance registryInstance = await applicationRegistryRepository.GetApplicationRegistryInstanceByID(applicationInstanceID).ConfigureAwait(false);

            registryInstance.ApplicationRegistryInstancesApplicationDiscoveryURLs.Clear(); //RSH 1/19/24 - does not clear DB rows
            foreach ((bool isNew, DomainModels.ApplicationDiscoveryURL applicationDiscoveryURL) in applicationDiscoveryInfo.ApplicationDiscoveryURLs)
            {
                ApplicationRegistryInstanceApplicationDiscoveryURL registryInstanceDiscoveryURL = new ApplicationRegistryInstanceApplicationDiscoveryURL()
                {
                    ApplicationRegistryInstance = registryInstance,
                    ApplicationDiscoveryURL = applicationDiscoveryURL
                };
                registryInstance.ApplicationRegistryInstancesApplicationDiscoveryURLs.Add(registryInstanceDiscoveryURL);
                applicationDiscoveryURL.ApplicationRegistryInstancesApplicationDiscoveryURLs.Clear(); //RSH 1/19/24 - does not clear DB rows
                applicationDiscoveryURL.ApplicationRegistryInstancesApplicationDiscoveryURLs.Add(registryInstanceDiscoveryURL);
            }
        }

        private void AddDiscoveryURLDiscoveryItemJoin(Model.ApplicationDiscoveryInfo applicationDiscoveryInfo)
        {
            applicationDiscoveryInfo.ApplicationDiscoveryItem.ApplicationDiscoveryURLApplicationDiscoveryItems.Clear(); //RSH 1/19/24 - does not clear DB rows
            foreach ((bool isNew, DomainModels.ApplicationDiscoveryURL applicationDiscoveryURL) in applicationDiscoveryInfo.ApplicationDiscoveryURLs)
            {
                ApplicationDiscoveryURLApplicationDiscoveryItem discoveryURLDiscoveryItem = new ApplicationDiscoveryURLApplicationDiscoveryItem()
                {
                    ApplicationDiscoveryURL = applicationDiscoveryURL,
                    ApplicationDiscoveryItem = applicationDiscoveryInfo.ApplicationDiscoveryItem
                };
                applicationDiscoveryInfo.ApplicationDiscoveryItem.ApplicationDiscoveryURLApplicationDiscoveryItems.Add(discoveryURLDiscoveryItem);
                applicationDiscoveryURL.ApplicationDiscoveryURLApplicationDiscoveryItems.Clear(); //RSH 1/19/24 - does not clear DB rows
                applicationDiscoveryURL.ApplicationDiscoveryURLApplicationDiscoveryItems.Add(discoveryURLDiscoveryItem);
            }
        }

        private void AddDiscoveryMethodDiscoveryItemJoin(Model.ApplicationDiscoveryInfo applicationDiscoveryInfo)
        {
            applicationDiscoveryInfo.ApplicationDiscoveryItem.ApplicationDiscoveryMethodsApplicationDiscoveryItems.Clear(); //RSH 1/19/24 - does not clear DB rows
            foreach ((bool isNew, DomainModels.ApplicationDiscoveryMethod applicationDiscoveryMethod) in applicationDiscoveryInfo.ApplicationDiscoveryMethods)
            {
                ApplicationDiscoveryMethodApplicationDiscoveryItem discoveryMethodDiscoveryItem = new ApplicationDiscoveryMethodApplicationDiscoveryItem()
                {
                    ApplicationDiscoveryMethod = applicationDiscoveryMethod,
                    ApplicationDiscoveryItem = applicationDiscoveryInfo.ApplicationDiscoveryItem
                };
                applicationDiscoveryInfo.ApplicationDiscoveryItem.ApplicationDiscoveryMethodsApplicationDiscoveryItems.Add(discoveryMethodDiscoveryItem);
                applicationDiscoveryMethod.ApplicationDiscoveryMethodsApplicationDiscoveryItems.Clear(); //RSH 1/19/24 - does not clear DB rows
                applicationDiscoveryMethod.ApplicationDiscoveryMethodsApplicationDiscoveryItems.Add(discoveryMethodDiscoveryItem);
            }
        }

        private async Task AddRegistryVersionDiscoveryItemJoin(Model.ApplicationDiscoveryInfo applicationDiscoveryInfo, int applicationVersionID)
        {
            await WaitForRepositoryToBeReady().ConfigureAwait(false);
            ApplicationRegistryRepository applicationRegistryRepository = new ApplicationRegistryRepository(Configuration);
            DomainModels.ApplicationRegistryVersion applicationRegistryVersion = await applicationRegistryRepository.GetApplicationRegistryVersionByID(applicationVersionID).ConfigureAwait(false);

            ApplicationRegistryVersionApplicationDiscoveryItem registryVersionDiscoveryItem = new ApplicationRegistryVersionApplicationDiscoveryItem()
            {
                ApplicationRegistryVersion = applicationRegistryVersion,
                ApplicationDiscoveryItem = applicationDiscoveryInfo.ApplicationDiscoveryItem
            };
            applicationRegistryVersion.ApplicationRegistryVersionApplicationDiscoveryItems.Clear(); //RSH 1/19/24 - does not clear DB rows
            applicationRegistryVersion.ApplicationRegistryVersionApplicationDiscoveryItems.Add(registryVersionDiscoveryItem);
            applicationDiscoveryInfo.ApplicationDiscoveryItem.ApplicationRegistryVersionsApplicationDiscoveryItems.Clear(); //RSH 1/19/24 - does not clear DB rows
            applicationDiscoveryInfo.ApplicationDiscoveryItem.ApplicationRegistryVersionsApplicationDiscoveryItems.Add(registryVersionDiscoveryItem);
        }

        public async Task SetURLsDown(int applicationInstanceID)
        {
            await WaitForRepositoryToBeReady().ConfigureAwait(false);
            List<ApplicationRegistryInstanceApplicationDiscoveryURL> applicationRegistryInstanceApplicationDiscoveryURLs = await ApplicationRegistryInstanceApplicationDiscoveryURLsRepository.GetAllByApplicationRegistryInstanceID(applicationInstanceID, null).ConfigureAwait(false);
            if (applicationRegistryInstanceApplicationDiscoveryURLs != null)
            {
                foreach (ApplicationRegistryInstanceApplicationDiscoveryURL applicationRegistryInstanceApplicationDiscoveryURL in applicationRegistryInstanceApplicationDiscoveryURLs)
                {
                    applicationRegistryInstanceApplicationDiscoveryURL.ApplicationDiscoveryURL.HealthStatus = "Down";
                    CycleDetector cycleDetector = new CycleDetector();
                    await ApplicationDiscoveryURLRepository.Save(applicationRegistryInstanceApplicationDiscoveryURL.ApplicationDiscoveryURL, cycleDetector).ConfigureAwait(false);
                }
            }
        }

        public async Task<ApplicationDiscoveryItem> GetDiscoveryItemByFriendlyName(string friendlyName)
        {
            await WaitForRepositoryToBeReady().ConfigureAwait(false);
            return await ApplicationDiscoveryItemRepository.GetByFriendlyName(friendlyName).ConfigureAwait(false);
        }

        //public Task<(string? currentHealthStatus, DateTimeOffset? lastStatusCheckDateTime)> GetCurrentHealthStatus(int iD)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task Save(DomainModels.ApplicationDiscoveryURL urlItem)
        {
            await WaitForRepositoryToBeReady().ConfigureAwait(false);
            CycleDetector cycleDetector = new CycleDetector();
            await ApplicationDiscoveryURLRepository.Save(urlItem, cycleDetector).ConfigureAwait(false);
        }

        public async Task<List<DomainModels.ApplicationDiscoveryURL>> GetURLs(bool healthy, DateTimeOffset withinTimeframeStart, bool joinWithInstance = true, bool instanceActive = true)
        {
            await WaitForRepositoryToBeReady().ConfigureAwait(false);
            return await ApplicationDiscoveryURLRepository.GetURLs(healthy, withinTimeframeStart, joinWithInstance, instanceActive).ConfigureAwait(false);
        }
    }
}
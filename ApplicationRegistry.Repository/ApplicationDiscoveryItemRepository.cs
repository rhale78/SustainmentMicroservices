using Common.DAL;
using ApplicationRegistry.DomainModels;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Repository
{
    internal class ApplicationDiscoveryItemRepository : RegistryRepositoryBase<ApplicationDiscoveryItem>
    {
        public ApplicationDiscoveryItemRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string TableName => "ApplicationDiscoveryItem";

        protected override List<string> FieldNames => new List<string>() { "ID", "FriendlyName", "ControllerName", "ControllerRoute" };

        protected override List<string> FieldSQLTypes => new List<string>() { "int", "nvarchar(max) null", "nvarchar(max) null", "nvarchar(max) null" };

        protected override List<object> GetFieldValues(ApplicationDiscoveryItem instance)
        {
            return new List<object>() { instance.ID, instance.FriendlyName, instance.ControllerName, instance.ControllerRoute };
        }

        public async Task<ApplicationDiscoveryItem> CreateOrFind(string friendlyName, string controllerName, string controllerRoute)
        {
            ApplicationDiscoveryItem instance = await base.ExecuteSQLQuerySingle($"SELECT * FROM {TableName} WHERE FriendlyName=@FriendlyName", new List<string>() { "@FriendlyName" }, new List<object>() { friendlyName }).ConfigureAwait(false);
            instance ??= new ApplicationDiscoveryItem()
            {
                FriendlyName = friendlyName,
                ControllerName = controllerName,
                ControllerRoute = controllerRoute
            };
            return instance;
        }

        public async Task<List<ApplicationDiscoveryItem>> GetAll()
        {
            List<ApplicationDiscoveryItem> instances = await base.GetAllInternal().ConfigureAwait(false);
            if (instances != null)
            {
                foreach (ApplicationDiscoveryItem instance in instances)
                {
                    instance.ApplicationDiscoveryMethodsApplicationDiscoveryItems = await new ApplicationDiscoveryMethodApplicationDiscoveryItemRepository(Configuration).GetAllByApplicationDiscoveryItemID(instance.ID).ConfigureAwait(false);
                    instance.ApplicationDiscoveryURLApplicationDiscoveryItems = await new ApplicationDiscoveryURLApplicationDiscoveryItemRepository(Configuration).GetAllByApplicationDiscoveryItemID(instance.ID).ConfigureAwait(false);
                }
            }
            return instances;
        }

        public async Task<ApplicationDiscoveryItem> GetByID(int id)
        {
            ApplicationDiscoveryItem instance = await base.GetByIDInternal(id).ConfigureAwait(false);
            if (instance != null)
            {
                instance.ApplicationDiscoveryMethodsApplicationDiscoveryItems = await new ApplicationDiscoveryMethodApplicationDiscoveryItemRepository(Configuration).GetAllByApplicationDiscoveryItemID(instance.ID).ConfigureAwait(false);
                instance.ApplicationDiscoveryURLApplicationDiscoveryItems = await new ApplicationDiscoveryURLApplicationDiscoveryItemRepository(Configuration).GetAllByApplicationDiscoveryItemID(instance.ID).ConfigureAwait(false);
            }
            return instance;
        }

        public async Task<ApplicationDiscoveryItem> GetByFriendlyName(string friendlyName)
        {
            ApplicationDiscoveryItem instance = await base.ExecuteSQLQuerySingle($"SELECT * FROM {TableName} WHERE FriendlyName=@FriendlyName", new List<string>() { "@FriendlyName" }, new List<object>() { friendlyName }).ConfigureAwait(false);
            if (instance != null)
            {
                instance.ApplicationDiscoveryMethodsApplicationDiscoveryItems = await new ApplicationDiscoveryMethodApplicationDiscoveryItemRepository(Configuration).GetAllByApplicationDiscoveryItemID(instance.ID).ConfigureAwait(false);
                instance.ApplicationDiscoveryURLApplicationDiscoveryItems = await new ApplicationDiscoveryURLApplicationDiscoveryItemRepository(Configuration).GetAllByApplicationDiscoveryItemID(instance.ID).ConfigureAwait(false);
            }
            return instance;
        }
        public async Task<ApplicationDiscoveryItem> Save(ApplicationDiscoveryItem instance, CycleDetector cycleDetector)
        {
            (int id, bool saveNeeded) = await base.SaveInternal(instance, cycleDetector).ConfigureAwait(false);

            if (saveNeeded)
            {
                instance.ApplicationDiscoveryMethodsApplicationDiscoveryItems = await new ApplicationDiscoveryMethodApplicationDiscoveryItemRepository(Configuration).SaveForApplicationDiscoveryItemID(instance.ApplicationDiscoveryMethodsApplicationDiscoveryItems, instance.ID, cycleDetector).ConfigureAwait(false);
                instance.ApplicationDiscoveryURLApplicationDiscoveryItems = await new ApplicationDiscoveryURLApplicationDiscoveryItemRepository(Configuration).SaveForApplicationDiscoveryItemID(instance.ApplicationDiscoveryURLApplicationDiscoveryItems, instance.ID, cycleDetector).ConfigureAwait(false);
                instance.ApplicationRegistryVersionsApplicationDiscoveryItems = await new ApplicationRegistryVersionApplicationDiscoveryItemRepository(Configuration).SaveForApplicationDiscoveryItemID(instance.ApplicationRegistryVersionsApplicationDiscoveryItems, instance.ID, cycleDetector).ConfigureAwait(false);
            }

            return instance;
        }

        protected override void LoadObjectInternal(ApplicationDiscoveryItem instance)
        {
            instance.ID = LoadInt();
            instance.FriendlyName = LoadString();
            instance.ControllerName = LoadString();
            instance.ControllerRoute = LoadString();
        }

        protected override Task PostTableCreate()
        {
            return Task.CompletedTask;
        }
    }
}
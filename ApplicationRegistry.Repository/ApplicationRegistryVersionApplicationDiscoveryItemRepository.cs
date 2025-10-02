using Common.DAL;
using ApplicationRegistry.DomainModels;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Repository
{
    internal class ApplicationRegistryVersionApplicationDiscoveryItemRepository : RegistryRepositoryBase<ApplicationRegistryVersionApplicationDiscoveryItem>
    {
        public ApplicationRegistryVersionApplicationDiscoveryItemRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string TableName => "ApplicationRegistryVersionApplicationDiscoveryItem";

        protected override List<string> FieldNames => new List<string>() { "ApplicationRegistryVersionID", "ApplicationDiscoveryItemID" };

        protected override List<string> FieldSQLTypes => new List<string>() { "int not null", "int not null" };

        protected override List<object> GetFieldValues(ApplicationRegistryVersionApplicationDiscoveryItem instance)
        {
            return new List<object>() { instance.ApplicationRegistryVersionID, instance.ApplicationDiscoveryItemID };
        }

        protected override void LoadObjectInternal(ApplicationRegistryVersionApplicationDiscoveryItem instance)
        {
            instance.ApplicationRegistryVersionID = LoadInt();
            instance.ApplicationDiscoveryItemID = LoadInt();
        }

        protected override Task PostTableCreate()
        {
            return Task.CompletedTask;
        }

        internal async Task<List<ApplicationRegistryVersionApplicationDiscoveryItem>> GetAllByApplicationRegistryVersionID(int applicationRegistryVersionID)
        {
            List<ApplicationRegistryVersionApplicationDiscoveryItem> items = await base.ExecuteSQLQuery($"SELECT * FROM {TableName} WHERE ApplicationRegistryVersionID=@ApplicationRegistryVersionID", new List<string>() { "@ApplicationRegistryVersionID" }, new List<object>() { applicationRegistryVersionID }).ConfigureAwait(false);
            if (items != null)
            {
                foreach (ApplicationRegistryVersionApplicationDiscoveryItem item in items)
                {
                    item.ApplicationDiscoveryItem = await new ApplicationDiscoveryItemRepository(Configuration).GetByIDInternal(item.ApplicationDiscoveryItemID).ConfigureAwait(false);
                }
            }
            return items;
        }

        internal async Task<List<ApplicationRegistryVersionApplicationDiscoveryItem>> SaveForApplicationDiscoveryItemID(List<ApplicationRegistryVersionApplicationDiscoveryItem> applicationRegistryVersionApplicationDiscoveryItems, int applicationDiscoveryItemID, CycleDetector cycleDetector)
        {
            if (applicationRegistryVersionApplicationDiscoveryItems != null)
            {
                foreach (ApplicationRegistryVersionApplicationDiscoveryItem applicationRegistryVersionApplicationDiscoveryItem in applicationRegistryVersionApplicationDiscoveryItems)
                {
                    ApplicationRegistryVersion applicationRegistryVersion = await new ApplicationRegistryVersionRepository(Configuration).Save(applicationRegistryVersionApplicationDiscoveryItem.ApplicationRegistryVersion, cycleDetector).ConfigureAwait(false);
                    applicationRegistryVersionApplicationDiscoveryItem.ApplicationRegistryVersionID = applicationRegistryVersion.ID;
                    applicationRegistryVersionApplicationDiscoveryItem.ApplicationDiscoveryItemID = applicationDiscoveryItemID;
                    await base.SaveInternal(applicationRegistryVersionApplicationDiscoveryItem, cycleDetector).ConfigureAwait(false);
                }
            }
            return applicationRegistryVersionApplicationDiscoveryItems;
        }

        internal async Task<List<ApplicationRegistryVersionApplicationDiscoveryItem>> SaveForApplicationRegistryVersionID(List<ApplicationRegistryVersionApplicationDiscoveryItem> applicationRegistryVersionApplicationDiscoveryItems, int applicationRegistryVersionID, CycleDetector cycleDetector)
        {
            if (applicationRegistryVersionApplicationDiscoveryItems != null)
            {
                foreach (ApplicationRegistryVersionApplicationDiscoveryItem applicationRegistryVersionApplicationDiscoveryItem in applicationRegistryVersionApplicationDiscoveryItems)
                {
                    applicationRegistryVersionApplicationDiscoveryItem.ApplicationRegistryVersionID = applicationRegistryVersionID;
                    ApplicationDiscoveryItem applicationDiscoveryItem = await new ApplicationDiscoveryItemRepository(Configuration).Save(applicationRegistryVersionApplicationDiscoveryItem.ApplicationDiscoveryItem, cycleDetector).ConfigureAwait(false);
                    applicationRegistryVersionApplicationDiscoveryItem.ApplicationDiscoveryItemID = applicationDiscoveryItem.ID;
                    await base.SaveInternal(applicationRegistryVersionApplicationDiscoveryItem, cycleDetector).ConfigureAwait(false);
                }
            }
            return applicationRegistryVersionApplicationDiscoveryItems;
        }
    }
}
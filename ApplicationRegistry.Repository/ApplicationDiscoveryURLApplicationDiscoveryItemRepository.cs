using Common.DAL;
using ApplicationRegistry.DomainModels;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Repository
{
    internal class ApplicationDiscoveryURLApplicationDiscoveryItemRepository : RegistryRepositoryBase<ApplicationDiscoveryURLApplicationDiscoveryItem>
    {
        public ApplicationDiscoveryURLApplicationDiscoveryItemRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string TableName => "ApplicationDiscoveryURLApplicationDiscoveryItem";

        protected override List<string> FieldNames => new List<string>() { "ApplicationDiscoveryURLID", "ApplicationDiscoveryItemID" };

        protected override List<string> FieldSQLTypes => new List<string>() { "int not null", "int not null" };

        protected override List<object> GetFieldValues(ApplicationDiscoveryURLApplicationDiscoveryItem instance)
        {
            return new List<object>() { instance.ApplicationDiscoveryURLID, instance.ApplicationDiscoveryItemID };
        }

        protected override void LoadObjectInternal(ApplicationDiscoveryURLApplicationDiscoveryItem instance)
        {
            instance.ApplicationDiscoveryURLID = LoadInt();
            instance.ApplicationDiscoveryItemID = LoadInt();
        }

        protected override Task PostTableCreate()
        {
            return Task.CompletedTask;
        }

        internal async Task<List<ApplicationDiscoveryURLApplicationDiscoveryItem>> GetAllByApplicationDiscoveryItemID(int applicationDiscoveryItemID)
        {
            List<ApplicationDiscoveryURLApplicationDiscoveryItem> items = await base.ExecuteSQLQuery($"SELECT * FROM {TableName} WHERE ApplicationDiscoveryItemID=@ApplicationDiscoveryItemID", new List<string>() { "@ApplicationDiscoveryItemID" }, new List<object>() { applicationDiscoveryItemID }).ConfigureAwait(false);
            if (items != null)
            {
                foreach (ApplicationDiscoveryURLApplicationDiscoveryItem item in items)
                {
                    item.ApplicationDiscoveryURL = await new ApplicationDiscoveryURLRepository(Configuration).GetByIDInternal(item.ApplicationDiscoveryURLID).ConfigureAwait(false);
                }
            }
            return items;
        }

        internal async Task<List<ApplicationDiscoveryURLApplicationDiscoveryItem>> SaveForApplicationDiscoveryItemID(List<ApplicationDiscoveryURLApplicationDiscoveryItem> applicationDiscoveryURLApplicationDiscoveryItems, int iD, CycleDetector cycleDetector)
        {
            if (applicationDiscoveryURLApplicationDiscoveryItems != null)
            {
                foreach (ApplicationDiscoveryURLApplicationDiscoveryItem applicationDiscoveryURLApplicationDiscoveryItem in applicationDiscoveryURLApplicationDiscoveryItems)
                {
                    applicationDiscoveryURLApplicationDiscoveryItem.ApplicationDiscoveryItemID = iD;
                    ApplicationDiscoveryURL applicationDiscoveryURL = await new ApplicationDiscoveryURLRepository(Configuration).Save(applicationDiscoveryURLApplicationDiscoveryItem.ApplicationDiscoveryURL, cycleDetector).ConfigureAwait(false);
                    applicationDiscoveryURLApplicationDiscoveryItem.ApplicationDiscoveryURLID = applicationDiscoveryURL.ID;
                    await base.SaveInternal(applicationDiscoveryURLApplicationDiscoveryItem, cycleDetector).ConfigureAwait(false);
                }
            }
            return applicationDiscoveryURLApplicationDiscoveryItems;
        }

        internal async Task<List<ApplicationDiscoveryURLApplicationDiscoveryItem>> SaveForApplicationDiscoveryURLID(List<ApplicationDiscoveryURLApplicationDiscoveryItem> applicationDiscoveryURLApplicationDiscoveryItems, int iD, CycleDetector cycleDetector)
        {
            if (applicationDiscoveryURLApplicationDiscoveryItems != null)
            {
                foreach (ApplicationDiscoveryURLApplicationDiscoveryItem applicationDiscoveryURLApplicationDiscoveryItem in applicationDiscoveryURLApplicationDiscoveryItems)
                {
                    applicationDiscoveryURLApplicationDiscoveryItem.ApplicationDiscoveryURLID = iD;
                    ApplicationDiscoveryItem applicationDiscoveryItem = await new ApplicationDiscoveryItemRepository(Configuration).Save(applicationDiscoveryURLApplicationDiscoveryItem.ApplicationDiscoveryItem, cycleDetector).ConfigureAwait(false);
                    applicationDiscoveryURLApplicationDiscoveryItem.ApplicationDiscoveryItemID = applicationDiscoveryItem.ID;
                    await base.SaveInternal(applicationDiscoveryURLApplicationDiscoveryItem, cycleDetector).ConfigureAwait(false);
                }
            }
            return applicationDiscoveryURLApplicationDiscoveryItems;
        }
    }
}
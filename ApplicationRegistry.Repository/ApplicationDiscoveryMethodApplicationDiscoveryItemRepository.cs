using Common.DAL;
using ApplicationRegistry.DomainModels;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Repository
{
    internal class ApplicationDiscoveryMethodApplicationDiscoveryItemRepository : RegistryRepositoryBase<ApplicationDiscoveryMethodApplicationDiscoveryItem>
    {
        protected override string TableName => "ApplicationDiscoveryMethodApplicationDiscoveryItem";

        protected override List<string> FieldNames => new List<string>() { "ApplicationDiscoveryMethodID", "ApplicationDiscoveryItemID" };

        protected override List<string> FieldSQLTypes => new List<string>() { "int not null", "int not null" };

        public ApplicationDiscoveryMethodApplicationDiscoveryItemRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override List<object> GetFieldValues(ApplicationDiscoveryMethodApplicationDiscoveryItem instance)
        {
            return new List<object>() { instance.ApplicationDiscoveryMethodID, instance.ApplicationDiscoveryItemID };
        }

        protected override void LoadObjectInternal(ApplicationDiscoveryMethodApplicationDiscoveryItem instance)
        {
            instance.ApplicationDiscoveryMethodID = LoadInt();
            instance.ApplicationDiscoveryItemID = LoadInt();
        }

        protected override Task PostTableCreate()
        {
            return Task.CompletedTask;
        }

        internal async Task<List<ApplicationDiscoveryMethodApplicationDiscoveryItem>> SaveForApplicationDiscoveryMethodID(List<ApplicationDiscoveryMethodApplicationDiscoveryItem> applicationDiscoveryMethodsApplicationDiscoveryItems, int applicationDiscoveryMethodID, CycleDetector cycleDetector)
        {
            if (applicationDiscoveryMethodsApplicationDiscoveryItems != null)
            {
                foreach (ApplicationDiscoveryMethodApplicationDiscoveryItem applicationDiscoveryMethodsApplicationDiscoveryItem in applicationDiscoveryMethodsApplicationDiscoveryItems)
                {
                    ApplicationDiscoveryItem applicationDiscoveryItem = await new ApplicationDiscoveryItemRepository(Configuration).Save(applicationDiscoveryMethodsApplicationDiscoveryItem.ApplicationDiscoveryItem, cycleDetector).ConfigureAwait(false);
                    applicationDiscoveryMethodsApplicationDiscoveryItem.ApplicationDiscoveryItemID = applicationDiscoveryItem.ID;
                    //ApplicationDiscoveryMethod applicationDiscoveryMethod = await new ApplicationDiscoveryMethodRepository(Configuration).Save(applicationDiscoveryMethodsApplicationDiscoveryItem.ApplicationDiscoveryMethod, cycleDetector);
                    applicationDiscoveryMethodsApplicationDiscoveryItem.ApplicationDiscoveryMethodID = applicationDiscoveryMethodID;
                    await base.SaveInternal(applicationDiscoveryMethodsApplicationDiscoveryItem, cycleDetector).ConfigureAwait(false);
                }
            }
            return applicationDiscoveryMethodsApplicationDiscoveryItems;
        }
        internal async Task<List<ApplicationDiscoveryMethodApplicationDiscoveryItem>> SaveForApplicationDiscoveryItemID(List<ApplicationDiscoveryMethodApplicationDiscoveryItem> applicationDiscoveryMethodsApplicationDiscoveryItems, int iD, CycleDetector cycleDetector)
        {
            if (applicationDiscoveryMethodsApplicationDiscoveryItems != null)
            {
                foreach (ApplicationDiscoveryMethodApplicationDiscoveryItem applicationDiscoveryMethodsApplicationDiscoveryItem in applicationDiscoveryMethodsApplicationDiscoveryItems)
                {
                    applicationDiscoveryMethodsApplicationDiscoveryItem.ApplicationDiscoveryItemID = iD;
                    ApplicationDiscoveryMethod applicationDiscoveryMethod = await new ApplicationDiscoveryMethodRepository(Configuration).Save(applicationDiscoveryMethodsApplicationDiscoveryItem.ApplicationDiscoveryMethod, cycleDetector).ConfigureAwait(false);
                    applicationDiscoveryMethodsApplicationDiscoveryItem.ApplicationDiscoveryMethodID = applicationDiscoveryMethod.ID;
                    await base.SaveInternal(applicationDiscoveryMethodsApplicationDiscoveryItem, cycleDetector).ConfigureAwait(false);
                }
            }
            return applicationDiscoveryMethodsApplicationDiscoveryItems;
        }
        internal async Task<List<ApplicationDiscoveryMethodApplicationDiscoveryItem>> GetAllByApplicationDiscoveryItemID(int applicationDiscoveryItemID)
        {
            List<ApplicationDiscoveryMethodApplicationDiscoveryItem> items = await base.ExecuteSQLQuery($"SELECT * FROM {TableName} WHERE ApplicationDiscoveryItemID=@ApplicationDiscoveryItemID", new List<string>() { "@ApplicationDiscoveryItemID" }, new List<object>() { applicationDiscoveryItemID }).ConfigureAwait(false);
            if (items != null)
            {
                foreach (ApplicationDiscoveryMethodApplicationDiscoveryItem item in items)
                {
                    item.ApplicationDiscoveryMethod = await new ApplicationDiscoveryMethodRepository(Configuration).GetByIDInternal(item.ApplicationDiscoveryMethodID).ConfigureAwait(false);
                }
            }
            return items;
        }
    }
}
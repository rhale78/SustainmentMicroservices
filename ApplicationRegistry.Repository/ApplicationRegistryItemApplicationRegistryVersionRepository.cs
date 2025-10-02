using Common.DAL;
using ApplicationRegistry.DomainModels;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Repository
{
    internal class ApplicationRegistryItemApplicationRegistryVersionRepository : RegistryRepositoryBase<ApplicationRegistryItemApplicationRegistryVersion>
    {
        public ApplicationRegistryItemApplicationRegistryVersionRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string TableName => "ApplicationRegistryItemApplicationRegistryVersion";

        protected override List<string> FieldNames => new List<string>() { "ApplicationRegistryItemID", "ApplicationRegistryVersionID" };

        protected override List<string> FieldSQLTypes => new List<string>() { "int not null", "int not null" };

        protected override List<object> GetFieldValues(ApplicationRegistryItemApplicationRegistryVersion instance)
        {
            return new List<object>() { instance.ApplicationRegistryItemID, instance.ApplicationRegistryVersionID };
        }

        protected override void LoadObjectInternal(ApplicationRegistryItemApplicationRegistryVersion instance)
        {
            instance.ApplicationRegistryItemID = LoadInt();
            instance.ApplicationRegistryVersionID = LoadInt();
        }

        protected override Task PostTableCreate()
        {
            return Task.CompletedTask;
        }

        //internal async Task CreateTable()
        //{
        //    throw new NotImplementedException();
        //}

        //internal async Task DropTable()
        //{
        //    await base.DropTable();
        //}

        internal async Task<List<ApplicationRegistryItemApplicationRegistryVersion>> GetAllByApplicationRegistryVersionID(int applicationRegistryVersionID)
        {
            List<ApplicationRegistryItemApplicationRegistryVersion> items = await base.ExecuteSQLQuery($"SELECT * FROM {TableName} WHERE ApplicationRegistryVersionID=@ApplicationRegistryVersionID", new List<string>() { "@ApplicationRegistryVersionID" }, new List<object>() { applicationRegistryVersionID }).ConfigureAwait(false);
            if (items != null)
            {
                foreach (ApplicationRegistryItemApplicationRegistryVersion item in items)
                {
                    item.ApplicationRegistryItem = await new ApplicationRegistryItemRepository(Configuration).GetByIDInternal(item.ApplicationRegistryItemID).ConfigureAwait(false);
                }
            }
            return items;
        }

        internal async Task<List<ApplicationRegistryItemApplicationRegistryVersion>> GetAllByApplicationRegistyItemID(int applicationRegistryItemID)
        {
            List<ApplicationRegistryItemApplicationRegistryVersion> items = await base.ExecuteSQLQuery($"SELECT * FROM {TableName} WHERE ApplicationRegistryItemID=@ApplicationRegistryItemID", new List<string>() { "@ApplicationRegistryItemID" }, new List<object>() { applicationRegistryItemID }).ConfigureAwait(false);
            if (items != null)
            {
                foreach (ApplicationRegistryItemApplicationRegistryVersion item in items)
                {
                    item.ApplicationRegistryVersion = await new ApplicationRegistryVersionRepository(Configuration).GetByIDInternal(item.ApplicationRegistryVersionID).ConfigureAwait(false);
                }
            }
            return items;
        }

        internal async Task<List<ApplicationRegistryItemApplicationRegistryVersion>> SaveForApplicationRegistryVersionID(List<ApplicationRegistryItemApplicationRegistryVersion> applicationRegistryItemApplicationRegistryVersions, int iD, CycleDetector cycleDetector)
        {
            if (applicationRegistryItemApplicationRegistryVersions != null)
            {
                foreach (ApplicationRegistryItemApplicationRegistryVersion applicationRegistryItemApplicationRegistryVersion in applicationRegistryItemApplicationRegistryVersions)
                {
                    applicationRegistryItemApplicationRegistryVersion.ApplicationRegistryVersionID = iD;
                    ApplicationRegistryItem applicationRegistryItem = await new ApplicationRegistryItemRepository(Configuration).Save(applicationRegistryItemApplicationRegistryVersion.ApplicationRegistryItem, cycleDetector).ConfigureAwait(false);
                    applicationRegistryItemApplicationRegistryVersion.ApplicationRegistryItemID = applicationRegistryItem.ID;
                    await base.SaveInternal(applicationRegistryItemApplicationRegistryVersion, cycleDetector).ConfigureAwait(false);
                }
            }
            return applicationRegistryItemApplicationRegistryVersions;
        }

        internal async Task<List<ApplicationRegistryItemApplicationRegistryVersion>> SaveForApplicationRegistyItemID(List<ApplicationRegistryItemApplicationRegistryVersion> applicationRegistryItemApplicationRegistryVersions, int iD, CycleDetector cycleDetector)
        {
            if (applicationRegistryItemApplicationRegistryVersions != null)
            {
                foreach (ApplicationRegistryItemApplicationRegistryVersion applicationRegistryItemApplicationRegistryVersion in applicationRegistryItemApplicationRegistryVersions)
                {
                    applicationRegistryItemApplicationRegistryVersion.ApplicationRegistryItemID = iD;
                    ApplicationRegistryVersion applicationRegistryVersion = await new ApplicationRegistryVersionRepository(Configuration).Save(applicationRegistryItemApplicationRegistryVersion.ApplicationRegistryVersion, cycleDetector).ConfigureAwait(false);
                    applicationRegistryItemApplicationRegistryVersion.ApplicationRegistryVersionID = applicationRegistryVersion.ID;
                    await base.SaveInternal(applicationRegistryItemApplicationRegistryVersion, cycleDetector).ConfigureAwait(false);
                }
            }
            return applicationRegistryItemApplicationRegistryVersions;
        }
    }
}
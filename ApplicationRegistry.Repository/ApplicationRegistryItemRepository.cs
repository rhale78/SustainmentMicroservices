using Common.DAL;
using ApplicationRegistry.DomainModels;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Repository
{
    internal class ApplicationRegistryItemRepository : RegistryRepositoryBase<ApplicationRegistryItem>
    {
        public ApplicationRegistryItemRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string TableName => "ApplicationRegistryItem";

        protected override List<string> FieldNames => new List<string>() { "ID", "ApplicationName", "FirstInstallDateTime" };

        protected override List<string> FieldSQLTypes => new List<string>() { "int", "nvarchar(max) null", "datetimeoffset null" };

        protected override List<object> GetFieldValues(ApplicationRegistryItem instance)
        {
            return new List<object>() { instance.ID, instance.ApplicationName, instance.FirstInstallDateTime };
        }

        protected override void LoadObjectInternal(ApplicationRegistryItem instance)
        {
            instance.ID = LoadInt();
            instance.ApplicationName = LoadString();
            instance.FirstInstallDateTime = LoadDateTimeOffset();
        }

        public async Task<ApplicationRegistryItem> CreateOrFind(string applicationName)
        {
            ApplicationRegistryItem instance = await base.ExecuteSQLQuerySingle($"SELECT * FROM {TableName} WHERE ApplicationName=@ApplicationName", new List<string>() { "@ApplicationName" }, new List<object>() { applicationName }).ConfigureAwait(false);
            instance ??= new ApplicationRegistryItem()
            {
                ApplicationName = applicationName,
                FirstInstallDateTime = DateTimeOffset.Now
            };
            return instance;
        }

        protected override Task PostTableCreate()
        {
            return Task.CompletedTask;
        }

        public async Task<List<ApplicationRegistryItem>> GetAll()
        {
            List<ApplicationRegistryItem> instances = await base.GetAllInternal().ConfigureAwait(false);
            if (instances != null)
            {
                foreach (ApplicationRegistryItem instance in instances)
                {
                    instance.ApplicationRegistryItemApplicationRegistryVersions = await new ApplicationRegistryItemApplicationRegistryVersionRepository(Configuration).GetAllByApplicationRegistyItemID(instance.ID).ConfigureAwait(false);
                }
            }
            return instances;
        }
        public async Task<ApplicationRegistryItem> GetByID(int id)
        {
            ApplicationRegistryItem instance = await base.GetByIDInternal(id).ConfigureAwait(false);
            if (instance != null)
            {
                instance.ApplicationRegistryItemApplicationRegistryVersions = await new ApplicationRegistryItemApplicationRegistryVersionRepository(Configuration).GetAllByApplicationRegistyItemID(instance.ID).ConfigureAwait(false);
            }
            return instance;
        }
        public async Task<ApplicationRegistryItem> Save(ApplicationRegistryItem instance, CycleDetector cycleDetector)
        {
            (int id, bool saveNeeded) = await base.SaveInternal(instance, cycleDetector).ConfigureAwait(false);
            if (saveNeeded)
            {
                instance.ApplicationRegistryItemApplicationRegistryVersions = await new ApplicationRegistryItemApplicationRegistryVersionRepository(Configuration).SaveForApplicationRegistyItemID(instance.ApplicationRegistryItemApplicationRegistryVersions, instance.ID, cycleDetector).ConfigureAwait(false);
            }

            return instance;
        }

    }
}
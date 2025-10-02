using Common.DAL;
using ApplicationRegistry.DomainModels;
using Log;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Repository
{
    internal class ApplicationDiscoveryURLRepository : RegistryRepositoryBase<ApplicationDiscoveryURL>
    {
        protected static LogInstance Log = LogInstance.CreateLog();
        public ApplicationDiscoveryURLRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string TableName => "ApplicationDiscoveryURL";

        protected override List<string> FieldNames => new List<string>() { "ID", "URL", "Port", "HealthStatus", "LastHealthStatusCheckDateTime" };

        protected override List<string> FieldSQLTypes => new List<string>() { "int", "nvarchar(max) null", "int null", "nvarchar(30) null", "datetimeoffset null" };

        protected override List<object> GetFieldValues(ApplicationDiscoveryURL instance)
        {
            return new List<object>() { instance.ID, instance.URL, instance.Port, instance.HealthStatus, instance.LastHealthStatusCheckDateTime };
        }

        public async Task<ApplicationDiscoveryURL> CreateOrFind(string url, int port)
        {
            ApplicationDiscoveryURL instance = await base.ExecuteSQLQuerySingle($"SELECT * FROM {TableName} WHERE URL=@URL AND Port=@Port", new List<string>() { "@URL", "@Port" }, new List<object>() { url, port }).ConfigureAwait(false);
            instance ??= new ApplicationDiscoveryURL()
            {
                URL = url,
                Port = port,
            };
            return instance;
        }

        protected override void LoadObjectInternal(ApplicationDiscoveryURL instance)
        {
            instance.ID = LoadInt();
            instance.URL = LoadString();
            instance.Port = LoadNullableInt();
            instance.HealthStatus = LoadString();
            instance.LastHealthStatusCheckDateTime = LoadNullableDateTimeOffset();
        }

        protected override Task PostTableCreate()
        {
            return Task.CompletedTask;
        }

        public async Task<List<ApplicationDiscoveryURL>> GetAll()
        {
            List<ApplicationDiscoveryURL> instances = await base.GetAllInternal().ConfigureAwait(false);
            if (instances != null)
            {
                foreach (ApplicationDiscoveryURL instance in instances)
                {
                    instance.ApplicationDiscoveryURLApplicationDiscoveryItems = await new ApplicationDiscoveryURLApplicationDiscoveryItemRepository(Configuration).GetAllByApplicationDiscoveryItemID(instance.ID).ConfigureAwait(false);
                    instance.ApplicationRegistryInstancesApplicationDiscoveryURLs = await new ApplicationRegistryInstanceApplicationDiscoveryURLsRepository(Configuration).GetAllByApplicationDiscoveryURLID(instance.ID).ConfigureAwait(false);
                }
            }
            return instances;
        }
        public async Task<ApplicationDiscoveryURL> GetByID(int id)
        {
            ApplicationDiscoveryURL instance = await base.GetByIDInternal(id).ConfigureAwait(false);
            if (instance != null)
            {
                instance.ApplicationDiscoveryURLApplicationDiscoveryItems = await new ApplicationDiscoveryURLApplicationDiscoveryItemRepository(Configuration).GetAllByApplicationDiscoveryItemID(instance.ID).ConfigureAwait(false);
                instance.ApplicationRegistryInstancesApplicationDiscoveryURLs = await new ApplicationRegistryInstanceApplicationDiscoveryURLsRepository(Configuration).GetAllByApplicationDiscoveryURLID(instance.ID).ConfigureAwait(false);
            }
            return instance;
        }
        public async Task<List<ApplicationDiscoveryURL>> Save(List<ApplicationDiscoveryURL> instances, CycleDetector cycleDetector)
        {
            List<ApplicationDiscoveryURL> urls = new List<ApplicationDiscoveryURL>();
            foreach (ApplicationDiscoveryURL instance in instances)
            {
                urls.Add(await Save(instance, cycleDetector).ConfigureAwait(false));
            }
            return urls;
        }
        public async Task<ApplicationDiscoveryURL> Save(ApplicationDiscoveryURL instance, CycleDetector cycleDetector)
        {
            //instance.ApplicationDiscoveryItem = await new ApplicationDiscoveryItemRepository(Configuration).Save(instance.ApplicationDiscoveryItem, cycleDetector);
            //instance.ApplicationDiscoveryItemID = instance.ApplicationDiscoveryItem?.ID;

            (int id, bool saveNeeded) = await base.SaveInternal(instance, cycleDetector).ConfigureAwait(false);

            if (saveNeeded)
            {
                instance.ApplicationDiscoveryURLApplicationDiscoveryItems = await new ApplicationDiscoveryURLApplicationDiscoveryItemRepository(Configuration).SaveForApplicationDiscoveryURLID(instance.ApplicationDiscoveryURLApplicationDiscoveryItems, instance.ID, cycleDetector).ConfigureAwait(false);
                instance.ApplicationRegistryInstancesApplicationDiscoveryURLs = await new ApplicationRegistryInstanceApplicationDiscoveryURLsRepository(Configuration).SaveForApplicationDiscoveryURLID(instance.ApplicationRegistryInstancesApplicationDiscoveryURLs, instance.ID, cycleDetector).ConfigureAwait(false);
            }

            return instance;
        }


        public async Task<(string? currentHealthStatus, DateTimeOffset? lastCheckDateTime)> GetCurrentHealthStatus(int id)
        {
            ApplicationDiscoveryURL instance = await GetByID(id).ConfigureAwait(false);
            return instance != null ? ((string? currentHealthStatus, DateTimeOffset? lastCheckDateTime))(instance.HealthStatus, instance.LastHealthStatusCheckDateTime) : ((string? currentHealthStatus, DateTimeOffset? lastCheckDateTime))(null, null);
        }

        internal async Task<List<ApplicationDiscoveryURL>> GetURLs(bool healthy, DateTimeOffset withinTimeframeStart, bool joinWithInstance = true, bool instanceActive = true)
        {
            string healthEquality = healthy ? "=" : "<>";
            //RSH 2/8/24 - enhancement from prior version - there is no need to check urls if the instance is down -
            //but we also may want all urls for instances that are down
            List<ApplicationDiscoveryURL> urls = new List<ApplicationDiscoveryURL>();
            urls = joinWithInstance
                ? await base.ExecuteSQLQuery(
                    $"SELECT t.* FROM {TableName} t WITH (NOLOCK) " +
                    $"inner join ApplicationRegistryInstanceApplicationDiscoveryURL it on t.id=it.ApplicationDiscoveryURLID " +
                    $"inner join ApplicationRegistryInstance i on i.ID=it.ApplicationRegistryInstanceID " +
                    $"WHERE i.IsActive=@InstanceActive AND (HealthStatus='' OR HealthStatus{healthEquality}'Healthy'  ) " +
                    $"AND (LastHealthStatusCheckDateTime is null or LastHealthStatusCheckDateTime='0001-01-01 00:00:00.0000000 +00:00' or LastHealthStatusCheckDateTime<@TimeSlotStart)",
                    new List<string>() { "@InstanceActive", "@TimeSlotStart" }, new List<object>() { instanceActive, withinTimeframeStart }).ConfigureAwait(false)
                : await base.ExecuteSQLQuery(
                        $"SELECT * FROM {TableName} WITH (NOLOCK) " +
                        $"WHERE (HealthStatus='' OR HealthStatus{healthEquality}'Healthy' )" +
                        $"AND (LastHealthStatusCheckDateTime is null or LastHealthStatusCheckDateTime='0001-01-01 00:00:00.0000000 +00:00' or LastHealthStatusCheckDateTime<@TimeSlotStart)",
                        new List<string>() { "@TimeSlotStart" }, new List<object>() { withinTimeframeStart }).ConfigureAwait(false);

            Log.LogDebug("GetURLs", $"healthy={healthy}, withinTimeframeStart={withinTimeframeStart}, joinWithInstance={joinWithInstance}, instanceActive={instanceActive}, urls={urls.Count}");
            return urls;
        }
    }
}
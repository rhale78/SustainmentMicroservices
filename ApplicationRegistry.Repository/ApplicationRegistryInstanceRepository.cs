using Common.DAL;
using ApplicationRegistry.DomainModels;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Repository
{
    internal class ApplicationRegistryInstanceRepository : RegistryRepositoryBase<ApplicationRegistryInstance>
    {
        protected override string TableName => "ApplicationRegistryInstance";

        protected override List<string> FieldNames => new List<string>() { "ID", "MachineName", "ApplicationPath", "NumberHeartbeats", "LastHeartbeatDateTime", "LastStartDateTime", "InstallDateTime", "IsActive" };

        protected override List<string> FieldSQLTypes => new List<string>() { "int", "nvarchar(max) null", "nvarchar(max) null", "int not null", "datetimeoffset null", "datetimeoffset not null", "datetimeoffset not null", "bit not null" };

        public ApplicationRegistryInstanceRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void LoadObjectInternal(ApplicationRegistryInstance instance)
        {
            instance.ID = LoadInt();
            instance.MachineName = LoadString();
            instance.ApplicationPath = LoadString();
            instance.NumberHeartbeats = LoadInt();
            instance.LastHeartbeatDateTime = LoadNullableDateTimeOffset();
            instance.LastStartDateTime = LoadDateTimeOffset();
            instance.InstallDateTime = LoadDateTimeOffset();
            instance.IsActive = LoadBoolean();
        }

        public async Task<ApplicationRegistryInstance> CreateOrFind(string applicationPath, string machineName, bool activeOnly = false)
        {
            string query = "SELECT * FROM ApplicationRegistryInstance WHERE ApplicationPath=@ApplicationPath AND MachineName=@MachineName";
            if (activeOnly)
            {
                query += " AND IsActive=1";
            }
            ApplicationRegistryInstance instance = await base.ExecuteSQLQuerySingle(query, new List<string>() { "@ApplicationPath", "@MachineName" }, new List<object>() { applicationPath, machineName }).ConfigureAwait(false);

            instance ??= new ApplicationRegistryInstance()
            {
                ApplicationPath = applicationPath,
                MachineName = machineName,
                InstallDateTime = DateTimeOffset.UtcNow,
                LastHeartbeatDateTime = DateTimeOffset.UtcNow,
                LastStartDateTime = DateTimeOffset.UtcNow,
                NumberHeartbeats = 1,
                IsActive = false
            };
            return instance;
        }

        public async Task<List<ApplicationRegistryInstance>> GetAll()
        {
            List<ApplicationRegistryInstance> instances = await base.GetAllInternal().ConfigureAwait(false);
            if (instances != null)
            {
                foreach (ApplicationRegistryInstance instance in instances)
                {
                    instance.ApplicationRegistryVersionApplicationRegistryInstances = await new ApplicationRegistryVersionApplicationRegistryInstanceRepository(Configuration).GetAllByApplicationRegistryInstanceID(instance.ID, instance).ConfigureAwait(false);
                    instance.ApplicationRegistryInstancesApplicationDiscoveryURLs = await new ApplicationRegistryInstanceApplicationDiscoveryURLsRepository(Configuration).GetAllByApplicationRegistryInstanceID(instance.ID, instance).ConfigureAwait(false);
                }
            }
            return instances;
        }

        //protected override string TableName => "ApplicationRegistryInstance";

        //protected override List<string> FieldNames => new List<string>() { "ID", "MachineName", "ApplicationPath", "NumberHeartbeats", "LastHeartbeatDateTime", "LastStartDateTime", "InstallDateTime", "IsActive" };

        //protected override List<string> FieldSQLTypes => new List<string>() { "int", "nvarchar(max) null", "nvarchar(max) null", "int not null", "datetimeoffset null", "datetimeoffset not null", "datetimeoffset not null", "bit not null" };

        //public ApplicationRegistryInstanceRepository(IConfiguration configuration) : base(configuration)
        //{
        //}

        //protected override void LoadObjectInternal(ApplicationRegistryInstance instance, object[] values)
        //{
        //    instance.ID = LoadInt(values, 0);
        //    instance.MachineName = LoadString(values, 1);
        //    instance.ApplicationPath = LoadString(values, 2);
        //    instance.NumberHeartbeats = LoadInt(values, 3);
        //    instance.LastHeartbeatDateTime = LoadNullableDateTimeOffset(values, 4);
        //    instance.LastStartDateTime = LoadDateTimeOffset(values, 5);
        //    instance.InstallDateTime = LoadDateTimeOffset(values, 6);
        //    instance.IsActive = LoadBoolean(values, 7);
        //}

        //public async Task<ApplicationRegistryInstance> CreateOrFind(string applicationPath, string machineName,bool activeOnly=false)
        //{
        //    string query = "SELECT * FROM ApplicationRegistryInstance WHERE ApplicationPath=@ApplicationPath AND MachineName=@MachineName";
        //    if(activeOnly)
        //    {
        //        query += " AND IsActive=1";
        //    }
        //    ApplicationRegistryInstance instance =  await base.ExecuteSQLQuerySingle(query, new List<string>() { "@ApplicationPath", "@MachineName" }, new List<object>() { applicationPath, machineName });

        //    if (instance == null)
        //    {
        //        instance = new ApplicationRegistryInstance()
        //        {
        //            ApplicationPath = applicationPath,
        //            MachineName = machineName,
        //            InstallDateTime = DateTimeOffset.UtcNow,
        //            LastHeartbeatDateTime = DateTimeOffset.UtcNow,
        //            LastStartDateTime = DateTimeOffset.UtcNow,
        //            NumberHeartbeats = 1,
        //            IsActive = false
        //        };
        //    }
        //    return instance;
        //}

        //public async Task<List<ApplicationRegistryInstance>> GetAll()
        //{
        //    List<ApplicationRegistryInstance> instances = await base.GetAllInternal();
        //    if (instances != null)
        //    {
        //        foreach (ApplicationRegistryInstance instance in instances)
        //        {
        //            instance.ApplicationRegistryVersionApplicationRegistryInstances = await new ApplicationRegistryVersionApplicationRegistryInstanceRepository(Configuration).GetAllByApplicationRegistryInstanceID(instance.ID, instance);
        //            instance.ApplicationRegistryInstancesApplicationDiscoveryURLs = await new ApplicationRegistryInstanceApplicationDiscoveryURLsRepository(Configuration).GetAllByApplicationRegistryInstanceID(instance.ID, instance);
        //        }
        //    }
        //    return instances;
        //}

        //public Task<ApplicationRegistryInfo> CreateOrFind(string applicationName, string applicationVersion, string applicationPath, string machineName, string applicationHash)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<ApplicationRegistryApplicationIDs> Save(ApplicationRegistryInfo applicationRegistryInfo)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<ApplicationRegistryInstance> Save(ApplicationRegistryInstance applicationRegistryInstance)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<ApplicationRegistryInstance> GetApplicationRegistryInstanceByID(int applicationInstanceID, bool activeOnly = false)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<ApplicationRegistryInstance> GetByID(int id)
        {
            ApplicationRegistryInstance instance = await base.GetByIDInternal(id).ConfigureAwait(false);
            if (instance != null)
            {
                instance.ApplicationRegistryVersionApplicationRegistryInstances = await new ApplicationRegistryVersionApplicationRegistryInstanceRepository(Configuration).GetAllByApplicationRegistryInstanceID(instance.ID, instance).ConfigureAwait(false);
                instance.ApplicationRegistryInstancesApplicationDiscoveryURLs = await new ApplicationRegistryInstanceApplicationDiscoveryURLsRepository(Configuration).GetAllByApplicationRegistryInstanceID(instance.ID, instance).ConfigureAwait(false);
            }
            return instance;
        }

        public async Task<ApplicationRegistryInstance> Save(ApplicationRegistryInstance instance, CycleDetector cycleDetector)
        {
            (int id, bool saveNeeded) = await base.SaveInternal(instance, cycleDetector).ConfigureAwait(false);

            if (saveNeeded)
            {
                instance.ApplicationRegistryInstancesApplicationDiscoveryURLs = await new ApplicationRegistryInstanceApplicationDiscoveryURLsRepository(Configuration).SaveForApplicationRegistryInstanceID(instance.ApplicationRegistryInstancesApplicationDiscoveryURLs, instance.ID, cycleDetector).ConfigureAwait(false);
                instance.ApplicationRegistryVersionApplicationRegistryInstances = await new ApplicationRegistryVersionApplicationRegistryInstanceRepository(Configuration).SaveForApplicationRegistryInstanceID(instance.ApplicationRegistryVersionApplicationRegistryInstances, instance.ID, cycleDetector).ConfigureAwait(false);
            }
            return instance;
        }

        public async Task<ApplicationRegistryInstance> IncrementHeartbeats(ApplicationRegistryInstance instance)
        {
            return await base.ExecuteSQLQuerySingle("UPDATE ApplicationRegistryInstances SET NumberHeartbeats=NumberHeartbeats+1,LastHeartbeatDateTime=SYSUTCDATETIME() OUTPUT inserted.* WHERE ID=@ID", new List<string>() { "@ID" }, new List<object>() { instance.ID }).ConfigureAwait(false);
        }
        public async Task<ApplicationRegistryInstance> SetActive(ApplicationRegistryInstance instance)
        {
            return await base.ExecuteSQLQuerySingle("UPDATE ApplicationRegistryInstances SET IsActive=1 OUTPUT inserted.* WHERE ID=@ID", new List<string>() { "@ID" }, new List<object>() { instance.ID }).ConfigureAwait(false);
        }
        public async Task<ApplicationRegistryInstance> SetInactive(ApplicationRegistryInstance instance)
        {
            return await base.ExecuteSQLQuerySingle("UPDATE ApplicationRegistryInstances SET IsActive=0 OUTPUT inserted.* WHERE ID=@ID", new List<string>() { "@ID" }, new List<object>() { instance.ID }).ConfigureAwait(false);
        }

        protected override List<object> GetFieldValues(ApplicationRegistryInstance instance)
        {
            return new List<object>() { instance.ID, instance.MachineName, instance.ApplicationPath, instance.NumberHeartbeats, instance.LastHeartbeatDateTime, instance.LastStartDateTime, instance.InstallDateTime, instance.IsActive };
        }

        protected override Task PostTableCreate()
        {
            return Task.CompletedTask;
        }
    }
}
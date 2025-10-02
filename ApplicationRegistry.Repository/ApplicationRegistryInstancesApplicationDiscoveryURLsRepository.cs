using Common.DAL;
using ApplicationRegistry.DomainModels;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Repository
{
    internal class ApplicationRegistryInstanceApplicationDiscoveryURLsRepository : RegistryRepositoryBase<ApplicationRegistryInstanceApplicationDiscoveryURL>
    {
        public ApplicationRegistryInstanceApplicationDiscoveryURLsRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string TableName => "ApplicationRegistryInstanceApplicationDiscoveryURL";

        protected override List<string> FieldNames => new List<string>() { "ApplicationRegistryInstanceID", "ApplicationDiscoveryURLID" };

        protected override List<string> FieldSQLTypes => new List<string>() { "int not null", "int not null" };

        internal async Task<List<ApplicationRegistryInstanceApplicationDiscoveryURL>> GetAllByApplicationRegistryInstanceID(int applicationInstanceID, ApplicationRegistryInstance instance)
        {
            List<ApplicationRegistryInstanceApplicationDiscoveryURL> applicationRegistryInstanceApplicationDiscoveryURLs = await base.ExecuteSQLQuery($"SELECT * FROM {TableName} WHERE ApplicationRegistryInstanceID=@ApplicationRegistryInstanceID", new List<string>() { "@ApplicationRegistryInstanceID" }, new List<object>() { applicationInstanceID }).ConfigureAwait(false);
            if (applicationRegistryInstanceApplicationDiscoveryURLs != null)
            {
                foreach (ApplicationRegistryInstanceApplicationDiscoveryURL applicationRegistryInstanceApplicationDiscoveryURL in applicationRegistryInstanceApplicationDiscoveryURLs)
                {
                    applicationRegistryInstanceApplicationDiscoveryURL.ApplicationRegistryInstance = instance;
                    applicationRegistryInstanceApplicationDiscoveryURL.ApplicationDiscoveryURL = await new ApplicationDiscoveryURLRepository(Configuration).GetByIDInternal(applicationRegistryInstanceApplicationDiscoveryURL.ApplicationDiscoveryURLID).ConfigureAwait(false);
                }
            }
            return applicationRegistryInstanceApplicationDiscoveryURLs;
        }

        internal async Task<List<ApplicationRegistryInstanceApplicationDiscoveryURL>> SaveForApplicationRegistryInstanceID(List<ApplicationRegistryInstanceApplicationDiscoveryURL> applicationRegistryInstancesApplicationDiscoveryURLs, int applicationInstanceID, CycleDetector cycleDetector)
        {
            if (applicationRegistryInstancesApplicationDiscoveryURLs != null)
            {
                foreach (ApplicationRegistryInstanceApplicationDiscoveryURL applicationRegistryInstanceApplicationDiscoveryURL in applicationRegistryInstancesApplicationDiscoveryURLs)
                {
                    applicationRegistryInstanceApplicationDiscoveryURL.ApplicationRegistryInstanceID = applicationInstanceID;
                    ApplicationDiscoveryURL discoveryURL = await new ApplicationDiscoveryURLRepository(Configuration).Save(applicationRegistryInstanceApplicationDiscoveryURL.ApplicationDiscoveryURL, cycleDetector).ConfigureAwait(false);
                    applicationRegistryInstanceApplicationDiscoveryURL.ApplicationDiscoveryURLID = discoveryURL.ID;
                    await base.SaveInternal(applicationRegistryInstanceApplicationDiscoveryURL, cycleDetector).ConfigureAwait(false);
                }
            }
            return applicationRegistryInstancesApplicationDiscoveryURLs;
        }

        protected override List<object> GetFieldValues(ApplicationRegistryInstanceApplicationDiscoveryURL instance)
        {
            return new List<object>() { instance.ApplicationRegistryInstanceID, instance.ApplicationDiscoveryURLID };
        }

        protected override void LoadObjectInternal(ApplicationRegistryInstanceApplicationDiscoveryURL instance)
        {
            instance.ApplicationRegistryInstanceID = LoadInt();
            instance.ApplicationDiscoveryURLID = LoadInt();
        }

        protected override Task PostTableCreate()
        {
            return Task.CompletedTask;
        }

        internal async Task<List<ApplicationRegistryInstanceApplicationDiscoveryURL>> SaveForApplicationDiscoveryURLID(List<ApplicationRegistryInstanceApplicationDiscoveryURL> applicationRegistryInstancesApplicationDiscoveryURLs, int iD, CycleDetector cycleDetector)
        {
            if (applicationRegistryInstancesApplicationDiscoveryURLs != null)
            {
                foreach (ApplicationRegistryInstanceApplicationDiscoveryURL applicationRegistryInstanceApplicationDiscoveryURL in applicationRegistryInstancesApplicationDiscoveryURLs)
                {
                    applicationRegistryInstanceApplicationDiscoveryURL.ApplicationDiscoveryURLID = iD;
                    ApplicationRegistryInstance applicationRegistryInstance = await new ApplicationRegistryInstanceRepository(Configuration).Save(applicationRegistryInstanceApplicationDiscoveryURL.ApplicationRegistryInstance, cycleDetector).ConfigureAwait(false);
                    applicationRegistryInstanceApplicationDiscoveryURL.ApplicationRegistryInstanceID = applicationRegistryInstance.ID;
                    await base.SaveInternal(applicationRegistryInstanceApplicationDiscoveryURL, cycleDetector).ConfigureAwait(false);
                }
            }
            return applicationRegistryInstancesApplicationDiscoveryURLs;
        }

        internal async Task<List<ApplicationRegistryInstanceApplicationDiscoveryURL>> GetAllByApplicationDiscoveryURLID(int applicationDiscoveryURLID)
        {
            List<ApplicationRegistryInstanceApplicationDiscoveryURL> items = await base.ExecuteSQLQuery($"SELECT * FROM {TableName} WHERE ApplicationDiscoveryURLID=@ApplicationDiscoveryURLID", new List<string>() { "@ApplicationDiscoveryURLID" }, new List<object>() { applicationDiscoveryURLID }).ConfigureAwait(false);
            if (items != null)
            {
                foreach (ApplicationRegistryInstanceApplicationDiscoveryURL item in items)
                {
                    item.ApplicationRegistryInstance = await new ApplicationRegistryInstanceRepository(Configuration).GetByIDInternal(item.ApplicationRegistryInstanceID).ConfigureAwait(false);
                }
            }
            return items;
        }
    }
}
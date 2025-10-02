using Common.DAL;
using ApplicationRegistry.DomainModels;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Repository
{

    internal class ApplicationRegistryVersionRepository : RegistryRepositoryBase<ApplicationRegistryVersion>
    {
        public ApplicationRegistryVersionRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string TableName => "ApplicationRegistryVersion";

        protected override List<string> FieldNames => new List<string>() { "ID", "ApplicationVersion", "ApplicationHash", "PreviousVersionID", "BuildDateTime", "FirstInstallDateTime" };

        protected override List<string> FieldSQLTypes => new List<string>() { "int", "narchar(max) null", "nvarchar(max) null", "int null", "datetimeoffset not null", "datetimeoffset not null" };

        public async Task<List<ApplicationRegistryVersion>> GetAll()
        {
            List<ApplicationRegistryVersion> versions = await base.GetAllInternal().ConfigureAwait(false);
            if (versions != null)
            {
                foreach (ApplicationRegistryVersion version in versions)
                {
                    version.ApplicationRegistryVersionApplicationDiscoveryItems = await new ApplicationRegistryVersionApplicationDiscoveryItemRepository(Configuration).GetAllByApplicationRegistryVersionID(version.ID).ConfigureAwait(false);
                    version.ApplicationRegistryVersionApplicationRegistryInstances = await new ApplicationRegistryVersionApplicationRegistryInstanceRepository(Configuration).GetAllByApplicationRegistryVersionID(version.ID).ConfigureAwait(false);
                    version.ApplicationRegistryItemApplicationRegistryVersions = await new ApplicationRegistryItemApplicationRegistryVersionRepository(Configuration).GetAllByApplicationRegistryVersionID(version.ID).ConfigureAwait(false);
                }
            }
            return versions;
        }

        public async Task<ApplicationRegistryVersion> GetByID(int applicationRegistryVersionID)
        {
            ApplicationRegistryVersion version = await base.GetByIDInternal(applicationRegistryVersionID).ConfigureAwait(false);
            if (version != null)
            {
                version.ApplicationRegistryVersionApplicationDiscoveryItems = await new ApplicationRegistryVersionApplicationDiscoveryItemRepository(Configuration).GetAllByApplicationRegistryVersionID(version.ID).ConfigureAwait(false);
                version.ApplicationRegistryVersionApplicationRegistryInstances = await new ApplicationRegistryVersionApplicationRegistryInstanceRepository(Configuration).GetAllByApplicationRegistryVersionID(version.ID).ConfigureAwait(false);
                version.ApplicationRegistryItemApplicationRegistryVersions = await new ApplicationRegistryItemApplicationRegistryVersionRepository(Configuration).GetAllByApplicationRegistryVersionID(version.ID).ConfigureAwait(false);
            }
            return version;
        }

        public async Task<ApplicationRegistryVersion> Save(ApplicationRegistryVersion version, CycleDetector cycleDetector)
        {
            (int id, bool saveNeeded) = await base.SaveInternal(version, cycleDetector).ConfigureAwait(false);

            if (saveNeeded)
            {
                version.ApplicationRegistryVersionApplicationDiscoveryItems = await new ApplicationRegistryVersionApplicationDiscoveryItemRepository(Configuration).SaveForApplicationRegistryVersionID(version.ApplicationRegistryVersionApplicationDiscoveryItems, version.ID, cycleDetector).ConfigureAwait(false);
                version.ApplicationRegistryVersionApplicationRegistryInstances = await new ApplicationRegistryVersionApplicationRegistryInstanceRepository(Configuration).SaveForApplicationRegistryVersionID(version.ApplicationRegistryVersionApplicationRegistryInstances, version.ID, cycleDetector).ConfigureAwait(false);
                version.ApplicationRegistryItemApplicationRegistryVersions = await new ApplicationRegistryItemApplicationRegistryVersionRepository(Configuration).SaveForApplicationRegistryVersionID(version.ApplicationRegistryItemApplicationRegistryVersions, version.ID, cycleDetector).ConfigureAwait(false);
            }
            return version;
        }

        protected override List<object> GetFieldValues(ApplicationRegistryVersion instance)
        {
            return new List<object>() { instance.ID, instance.ApplicationVersion, instance.ApplicationHash, instance.PreviousVersionID, instance.BuildDateTime, instance.FirstInstallDateTime };
        }

        protected override void LoadObjectInternal(ApplicationRegistryVersion instance)
        {
            instance.ID = LoadInt();
            instance.ApplicationVersion = LoadString();
            instance.ApplicationHash = LoadString();
            instance.PreviousVersionID = LoadNullableInt();
            instance.BuildDateTime = LoadDateTimeOffset();
            instance.FirstInstallDateTime = LoadDateTimeOffset();
        }

        public async Task<ApplicationRegistryVersion> CreateOrFind(string applicationVersion, string applicationHash)
        {
            ApplicationRegistryVersion applicationRegistryVersion = await base.ExecuteSQLQuerySingle($"SELECT * FROM {TableName} WHERE ApplicationVersion=@ApplicationVersion AND ApplicationHash=@ApplicationHash", new List<string>() { "@ApplicationVersion", "@ApplicationHash" }, new List<object>() { applicationVersion, applicationHash }).ConfigureAwait(false);
            applicationRegistryVersion ??= new ApplicationRegistryVersion()
            {
                ApplicationVersion = applicationVersion,
                ApplicationHash = applicationHash,
                BuildDateTime = DateTimeOffset.UtcNow,
                FirstInstallDateTime = DateTimeOffset.UtcNow
            };
            return applicationRegistryVersion;
        }
        protected override Task PostTableCreate()
        {
            return Task.CompletedTask;
        }
    }
}
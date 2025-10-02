using Common.DAL;
using ApplicationRegistry.DomainModels;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Repository
{
    internal class ApplicationRegistryVersionApplicationRegistryInstanceRepository : RegistryRepositoryBase<ApplicationRegistryVersionApplicationRegistryInstance>
    {
        public ApplicationRegistryVersionApplicationRegistryInstanceRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string TableName => "ApplicationRegistryVersionApplicationRegistryInstance";

        protected override List<string> FieldNames => new List<string>() { "ApplicationRegistryVersionID", "ApplicationRegistryInstanceID" };

        protected override List<string> FieldSQLTypes => new List<string>() { "int not null", "int not null" };

        internal async Task<List<ApplicationRegistryVersionApplicationRegistryInstance>> GetAllByApplicationRegistryInstanceID(int applicationInstanceID, ApplicationRegistryInstance instance)
        {
            List<ApplicationRegistryVersionApplicationRegistryInstance> applicationRegistryVersionApplicationRegistryInstances = await base.ExecuteSQLQuery($"SELECT * FROM {TableName} WHERE ApplicationRegistryInstanceID=@ApplicationRegistryInstanceID", new List<string>() { "@ApplicationRegistryInstanceID" }, new List<object>() { applicationInstanceID }).ConfigureAwait(false);
            if (applicationRegistryVersionApplicationRegistryInstances != null)
            {
                foreach (ApplicationRegistryVersionApplicationRegistryInstance applicationRegistryVersionApplicationRegistryInstance in applicationRegistryVersionApplicationRegistryInstances)
                {
                    applicationRegistryVersionApplicationRegistryInstance.ApplicationRegistryInstance = instance;
                    applicationRegistryVersionApplicationRegistryInstance.ApplicationRegistryVersion = await new ApplicationRegistryVersionRepository(Configuration).GetByID(applicationRegistryVersionApplicationRegistryInstance.ApplicationRegistryVersionID).ConfigureAwait(false);
                }
            }
            return applicationRegistryVersionApplicationRegistryInstances;
        }

        internal async Task<List<ApplicationRegistryVersionApplicationRegistryInstance>> SaveForApplicationRegistryInstanceID(List<ApplicationRegistryVersionApplicationRegistryInstance> applicationRegistryVersionApplicationRegistryInstances, int applicationInstanceID, CycleDetector cycleDetector)
        {
            foreach (ApplicationRegistryVersionApplicationRegistryInstance applicationRegistryVersionApplicationRegistryInstance in applicationRegistryVersionApplicationRegistryInstances)
            {
                applicationRegistryVersionApplicationRegistryInstance.ApplicationRegistryInstanceID = applicationInstanceID;
                ApplicationRegistryVersion applicationRegistryVersion = await new ApplicationRegistryVersionRepository(Configuration).Save(applicationRegistryVersionApplicationRegistryInstance.ApplicationRegistryVersion, cycleDetector).ConfigureAwait(false);
                applicationRegistryVersionApplicationRegistryInstance.ApplicationRegistryVersionID = applicationRegistryVersion.ID;
                await base.SaveInternal(applicationRegistryVersionApplicationRegistryInstance, cycleDetector).ConfigureAwait(false);
            }
            return applicationRegistryVersionApplicationRegistryInstances;
        }

        protected override List<object> GetFieldValues(ApplicationRegistryVersionApplicationRegistryInstance instance)
        {
            return new List<object>() { instance.ApplicationRegistryVersionID, instance.ApplicationRegistryInstanceID };
        }

        protected override void LoadObjectInternal(ApplicationRegistryVersionApplicationRegistryInstance instance)
        {
            instance.ApplicationRegistryVersionID = LoadInt();
            instance.ApplicationRegistryInstanceID = LoadInt();
        }

        protected override Task PostTableCreate()
        {
            return Task.CompletedTask;
        }

        internal async Task<List<ApplicationRegistryVersionApplicationRegistryInstance>> SaveForApplicationRegistryVersionID(List<ApplicationRegistryVersionApplicationRegistryInstance> applicationRegistryVersionApplicationRegistryInstances, int iD, CycleDetector cycleDetector)
        {
            if (applicationRegistryVersionApplicationRegistryInstances != null)
            {
                foreach (ApplicationRegistryVersionApplicationRegistryInstance applicationRegistryVersionApplicationRegistryInstance in applicationRegistryVersionApplicationRegistryInstances)
                {
                    applicationRegistryVersionApplicationRegistryInstance.ApplicationRegistryVersionID = iD;
                    ApplicationRegistryInstance applicationRegistryInstance = await new ApplicationRegistryInstanceRepository(Configuration).Save(applicationRegistryVersionApplicationRegistryInstance.ApplicationRegistryInstance, cycleDetector).ConfigureAwait(false);
                    applicationRegistryVersionApplicationRegistryInstance.ApplicationRegistryInstanceID = applicationRegistryInstance.ID;
                    await base.SaveInternal(applicationRegistryVersionApplicationRegistryInstance, cycleDetector).ConfigureAwait(false);
                }
            }
            return applicationRegistryVersionApplicationRegistryInstances;
        }

        internal async Task<List<ApplicationRegistryVersionApplicationRegistryInstance>> GetAllByApplicationRegistryVersionID(int iD)
        {
            List<ApplicationRegistryVersionApplicationRegistryInstance> items = await base.ExecuteSQLQuery($"SELECT * FROM {TableName} WHERE ApplicationRegistryVersionID=@ApplicationRegistryVersionID", new List<string>() { "@ApplicationRegistryVersionID" }, new List<object>() { iD }).ConfigureAwait(false);
            if (items != null)
            {
                foreach (ApplicationRegistryVersionApplicationRegistryInstance item in items)
                {
                    item.ApplicationRegistryInstance = await new ApplicationRegistryInstanceRepository(Configuration).GetByIDInternal(item.ApplicationRegistryInstanceID).ConfigureAwait(false);
                }
            }
            return items;
        }
    }
}
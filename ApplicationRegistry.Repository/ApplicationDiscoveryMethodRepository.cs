using Common.DAL;
using ApplicationRegistry.DomainModels;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Repository
{
    internal class ApplicationDiscoveryMethodRepository : RegistryRepositoryBase<ApplicationDiscoveryMethod>
    {
        public ApplicationDiscoveryMethodRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string TableName => "ApplicationDiscoveryMethod";

        protected override List<string> FieldNames => new List<string>() { "ID", "HttpMethod", "MethodName", "Template" };

        protected override List<string> FieldSQLTypes => new List<string>() { "int", "nvarchar(max) null", "nvarchar(max) null", "nvarchar(max) null" };

        protected override List<object> GetFieldValues(ApplicationDiscoveryMethod instance)
        {
            return new List<object>() { instance.ID, instance.HttpMethod, instance.MethodName, instance.Template };
        }

        protected override void LoadObjectInternal(ApplicationDiscoveryMethod instance)
        {
            instance.ID = LoadInt();
            instance.HttpMethod = LoadString();
            instance.MethodName = LoadString();
            instance.Template = LoadString();
        }

        public async Task<ApplicationDiscoveryMethod> CreateOrFind(string httpMethod, string methodName, string template)
        {
            ApplicationDiscoveryMethod instance = await base.ExecuteSQLQuerySingle($"SELECT * FROM {TableName} WHERE HttpMethod=@HttpMethod AND MethodName=@MethodName AND Template=@Template", new List<string>() { "@HttpMethod", "@MethodName", "@Template" }, new List<object>() { httpMethod, methodName, template }).ConfigureAwait(false);
            instance ??= new ApplicationDiscoveryMethod()
            {
                HttpMethod = httpMethod,
                MethodName = methodName,
                Template = template
            };
            return instance;
        }

        protected override Task PostTableCreate()
        {
            return Task.CompletedTask;
        }
        public async Task<List<ApplicationDiscoveryMethod>> GetAll()
        {
            List<ApplicationDiscoveryMethod> instances = await base.GetAllInternal().ConfigureAwait(false);
            if (instances != null)
            {
                foreach (ApplicationDiscoveryMethod instance in instances)
                {
                    instance.ApplicationDiscoveryMethodsApplicationDiscoveryItems = await new ApplicationDiscoveryMethodApplicationDiscoveryItemRepository(Configuration).GetAllByApplicationDiscoveryItemID(instance.ID).ConfigureAwait(false);
                }
            }
            return instances;
        }
        public async Task<ApplicationDiscoveryMethod> GetByID(int id)
        {
            ApplicationDiscoveryMethod instance = await base.GetByIDInternal(id).ConfigureAwait(false);
            if (instance != null)
            {
                instance.ApplicationDiscoveryMethodsApplicationDiscoveryItems = await new ApplicationDiscoveryMethodApplicationDiscoveryItemRepository(Configuration).GetAllByApplicationDiscoveryItemID(instance.ID).ConfigureAwait(false);
            }
            return instance;
        }
        public async Task<ApplicationDiscoveryMethod> Save(ApplicationDiscoveryMethod instance, CycleDetector cycleDetector)
        {
            (int id, bool saveNeeded) = await base.SaveInternal(instance, cycleDetector).ConfigureAwait(false);

            if (saveNeeded)
            {
                instance.ApplicationDiscoveryMethodsApplicationDiscoveryItems = await new ApplicationDiscoveryMethodApplicationDiscoveryItemRepository(Configuration).SaveForApplicationDiscoveryMethodID(instance.ApplicationDiscoveryMethodsApplicationDiscoveryItems, instance.ID, cycleDetector).ConfigureAwait(false);
            }

            return instance;
        }
    }
}
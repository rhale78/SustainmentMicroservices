using Common.DAL;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegistry.Repository
{
    public abstract class RegistryRepositoryBase<T> : DALObjectBase<T> where T : new()
    {
        public RegistryRepositoryBase(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string ConnectionString => Configuration.GetConnectionString("ApplicationRegistry");
    }

}